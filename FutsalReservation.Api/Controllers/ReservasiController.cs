using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FutsalReservation.Api.Data;
using FutsalReservation.Api.Dtos;
using FutsalReservation.Api.Models;

namespace FutsalReservation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservasiController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReservasiController(AppDbContext db)
    {
        _db = db;
    }

    private int UserIdSekarang => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin => User.IsInRole("Admin");

    [HttpGet]
    public async Task<IActionResult> GetSemua()
    {
        var query = _db.Reservasi
            .Include(r => r.User)
            .Include(r => r.Lapangan)
            .Include(r => r.Pembayaran)
            .AsQueryable();

        // user biasa hanya boleh lihat reservasi miliknya sendiri
        if (!IsAdmin)
            query = query.Where(r => r.UserId == UserIdSekarang);

        var data = await query
            .OrderByDescending(r => r.DibuatPada)
            .Select(r => MapResponse(r))
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSatu(int id)
    {
        var reservasi = await _db.Reservasi
            .Include(r => r.User)
            .Include(r => r.Lapangan)
            .Include(r => r.Pembayaran)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservasi == null)
            return NotFound(new { message = "Reservasi tidak ditemukan." });

        if (!IsAdmin && reservasi.UserId != UserIdSekarang)
            return Forbid();

        return Ok(MapResponse(reservasi));
    }

    // daftar jam yang sudah terisi untuk satu lapangan pada satu tanggal
    [HttpGet("ketersediaan")]
    public async Task<IActionResult> Ketersediaan([FromQuery] int lapanganId, [FromQuery] DateTime tanggal)
    {
        var daftar = await _db.Reservasi
            .Where(r => r.LapanganId == lapanganId
                        && r.Tanggal == tanggal.Date
                        && r.Status != "Dibatalkan")
            .Select(r => new { r.JamMulai, r.JamSelesai })
            .ToListAsync();

        var terisi = new SortedSet<int>();
        foreach (var r in daftar)
            for (int h = r.JamMulai; h < r.JamSelesai; h++)
                terisi.Add(h);

        return Ok(new KetersediaanResponse
        {
            LapanganId = lapanganId,
            Tanggal = tanggal.Date,
            SlotTerisi = terisi.ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Buat(ReservasiRequest req)
    {
        if (req.JamSelesai <= req.JamMulai)
            return BadRequest(new { message = "Jam selesai harus lebih besar dari jam mulai." });

        if (req.Tanggal.Date < DateTime.Today)
            return BadRequest(new { message = "Tanggal reservasi tidak boleh di masa lalu." });

        var lapangan = await _db.Lapangan.FindAsync(req.LapanganId);
        if (lapangan == null || !lapangan.Aktif)
            return BadRequest(new { message = "Lapangan tidak tersedia." });

        // cek bentrok jadwal pada lapangan & tanggal yang sama
        bool bentrok = await _db.Reservasi.AnyAsync(r =>
            r.LapanganId == req.LapanganId &&
            r.Tanggal == req.Tanggal.Date &&
            r.Status != "Dibatalkan" &&
            req.JamMulai < r.JamSelesai &&
            r.JamMulai < req.JamSelesai);

        if (bentrok)
            return Conflict(new { message = "Jadwal pada jam tersebut sudah dipesan. Silakan pilih jam lain." });

        int durasi = req.JamSelesai - req.JamMulai;
        var total = durasi * lapangan.HargaPerJam;

        var reservasi = new Reservasi
        {
            UserId = UserIdSekarang,
            LapanganId = req.LapanganId,
            Tanggal = req.Tanggal.Date,
            JamMulai = req.JamMulai,
            JamSelesai = req.JamSelesai,
            TotalHarga = total,
            Status = "Menunggu",
            Catatan = req.Catatan,
            Pembayaran = new Pembayaran
            {
                Jumlah = total,
                Metode = "-",
                Status = "Belum Dibayar"
            }
        };

        _db.Reservasi.Add(reservasi);
        await _db.SaveChangesAsync();

        await _db.Entry(reservasi).Reference(r => r.Lapangan).LoadAsync();
        await _db.Entry(reservasi).Reference(r => r.User).LoadAsync();

        return CreatedAtAction(nameof(GetSatu), new { id = reservasi.Id }, MapResponse(reservasi));
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UbahStatus(int id, UbahStatusRequest req)
    {
        var reservasi = await _db.Reservasi
            .Include(r => r.User)
            .Include(r => r.Lapangan)
            .Include(r => r.Pembayaran)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservasi == null)
            return NotFound(new { message = "Reservasi tidak ditemukan." });

        var status = req.Status.Trim();
        var statusValid = new[] { "Menunggu", "Dikonfirmasi", "Dibatalkan", "Selesai" };
        if (!statusValid.Contains(status))
            return BadRequest(new { message = "Status tidak dikenali." });

        // user biasa hanya boleh membatalkan reservasi miliknya sendiri
        if (!IsAdmin)
        {
            if (reservasi.UserId != UserIdSekarang)
                return Forbid();

            if (status != "Dibatalkan")
                return BadRequest(new { message = "Anda hanya dapat membatalkan reservasi." });

            // setelah dikonfirmasi/lunas, user tidak boleh membatalkan
            if (reservasi.Status != "Menunggu")
                return BadRequest(new { message = "Reservasi yang sudah dikonfirmasi tidak dapat dibatalkan. Silakan hubungi admin." });
        }

        reservasi.Status = status;

        // jika dibatalkan dan pembayaran belum lunas, kembalikan ke kondisi awal
        // supaya tidak ikut muncul di antrean verifikasi admin
        if (status == "Dibatalkan" && reservasi.Pembayaran != null && reservasi.Pembayaran.Status != "Lunas")
        {
            reservasi.Pembayaran.Status = "Belum Dibayar";
            reservasi.Pembayaran.Metode = "-";
        }

        await _db.SaveChangesAsync();

        return Ok(MapResponse(reservasi));
    }

    private static ReservasiResponse MapResponse(Reservasi r) => new()
    {
        Id = r.Id,
        UserId = r.UserId,
        NamaUser = r.User?.Nama ?? string.Empty,
        LapanganId = r.LapanganId,
        NamaLapangan = r.Lapangan?.Nama ?? string.Empty,
        Tanggal = r.Tanggal,
        JamMulai = r.JamMulai,
        JamSelesai = r.JamSelesai,
        TotalHarga = r.TotalHarga,
        Status = r.Status,
        Catatan = r.Catatan,
        DibuatPada = r.DibuatPada,
        StatusPembayaran = r.Pembayaran?.Status ?? "Belum Dibayar",
        MetodePembayaran = r.Pembayaran != null && r.Pembayaran.Metode != "-" ? r.Pembayaran.Metode : null,
        KodeBooking = r.Pembayaran?.KodeBooking,
        BuktiTransfer = r.Pembayaran?.BuktiTransfer
    };
}
