using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FutsalReservation.Api.Data;
using FutsalReservation.Api.Dtos;

namespace FutsalReservation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PembayaranController : ControllerBase
{
    private readonly AppDbContext _db;

    public PembayaranController(AppDbContext db)
    {
        _db = db;
    }

    private int UserIdSekarang => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin => User.IsInRole("Admin");

    [HttpGet]
    public async Task<IActionResult> GetSemua()
    {
        var query = _db.Pembayaran
            .Include(p => p.Reservasi).ThenInclude(r => r!.Lapangan)
            .Include(p => p.Reservasi).ThenInclude(r => r!.User)
            .AsQueryable();

        if (!IsAdmin)
            query = query.Where(p => p.Reservasi!.UserId == UserIdSekarang);

        var data = await query
            .OrderByDescending(p => p.DibuatPada)
            .Select(p => MapResponse(p))
            .ToListAsync();

        return Ok(data);
    }

    // user mengirim pembayaran untuk reservasinya
    [HttpPost]
    public async Task<IActionResult> Bayar(BayarRequest req)
    {
        var pembayaran = await _db.Pembayaran
            .Include(p => p.Reservasi).ThenInclude(r => r!.Lapangan)
            .Include(p => p.Reservasi).ThenInclude(r => r!.User)
            .FirstOrDefaultAsync(p => p.ReservasiId == req.ReservasiId);

        if (pembayaran?.Reservasi == null)
            return NotFound(new { message = "Data pembayaran tidak ditemukan." });

        if (!IsAdmin && pembayaran.Reservasi.UserId != UserIdSekarang)
            return Forbid();

        if (pembayaran.Reservasi.Status == "Dibatalkan")
            return BadRequest(new { message = "Reservasi sudah dibatalkan." });

        if (pembayaran.Status == "Lunas")
            return BadRequest(new { message = "Pembayaran sudah lunas." });

        var metode = await _db.MetodePembayaran.FindAsync(req.MetodePembayaranId);
        if (metode == null || !metode.Aktif)
            return BadRequest(new { message = "Metode pembayaran tidak tersedia." });

        // transfer bank / e-wallet wajib menyertakan bukti transfer
        if (metode.Tipe != "Tunai" && string.IsNullOrWhiteSpace(req.BuktiTransfer))
            return BadRequest(new { message = "Bukti transfer wajib diunggah untuk metode " + metode.Tipe + "." });

        pembayaran.Metode = LabelMetode(metode);
        pembayaran.Status = "Menunggu Verifikasi";
        pembayaran.TanggalBayar = DateTime.Now;
        pembayaran.BuktiTransfer = string.IsNullOrWhiteSpace(req.BuktiTransfer) ? null : req.BuktiTransfer.Trim();

        // pembayaran tunai/bayar di tempat mendapat kode booking untuk ditunjukkan saat datang
        if (metode.Tipe == "Tunai")
            pembayaran.KodeBooking = BuatKodeBooking(pembayaran.ReservasiId);

        await _db.SaveChangesAsync();
        return Ok(MapResponse(pembayaran));
    }

    // gabungkan info metode menjadi satu label lengkap, mis. "BCA · 1234567890 · a.n. Futsal Arena"
    private static string LabelMetode(Models.MetodePembayaran m)
    {
        if (m.Tipe == "Tunai")
            return m.Nama;

        var bagian = new List<string> { m.Nama };
        if (!string.IsNullOrWhiteSpace(m.NomorAkun))
            bagian.Add(m.NomorAkun.Trim());
        if (!string.IsNullOrWhiteSpace(m.AtasNama))
            bagian.Add("a.n. " + m.AtasNama.Trim());

        return string.Join(" · ", bagian);
    }

    private static string BuatKodeBooking(int reservasiId)
    {
        const string huruf = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var acak = new string(Enumerable.Range(0, 4)
            .Select(_ => huruf[Random.Shared.Next(huruf.Length)]).ToArray());
        return $"BK-{reservasiId:D4}-{acak}";
    }

    // admin memverifikasi pembayaran -> reservasi otomatis dikonfirmasi
    [HttpPut("{id:int}/verifikasi")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Verifikasi(int id)
    {
        var pembayaran = await _db.Pembayaran
            .Include(p => p.Reservasi).ThenInclude(r => r!.Lapangan)
            .Include(p => p.Reservasi).ThenInclude(r => r!.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pembayaran?.Reservasi == null)
            return NotFound(new { message = "Data pembayaran tidak ditemukan." });

        if (pembayaran.Status != "Menunggu Verifikasi")
            return BadRequest(new { message = "Pembayaran belum dikirim oleh pemesan." });

        if (pembayaran.Reservasi.Status == "Dibatalkan")
            return BadRequest(new { message = "Reservasi sudah dibatalkan, pembayaran tidak dapat diverifikasi." });

        pembayaran.Status = "Lunas";
        pembayaran.Reservasi.Status = "Dikonfirmasi";

        await _db.SaveChangesAsync();
        return Ok(MapResponse(pembayaran));
    }

    private static PembayaranResponse MapResponse(Models.Pembayaran p) => new()
    {
        Id = p.Id,
        ReservasiId = p.ReservasiId,
        NamaLapangan = p.Reservasi?.Lapangan?.Nama ?? string.Empty,
        NamaUser = p.Reservasi?.User?.Nama ?? string.Empty,
        Jumlah = p.Jumlah,
        Metode = p.Metode,
        Status = p.Status,
        StatusReservasi = p.Reservasi?.Status ?? string.Empty,
        KodeBooking = p.KodeBooking,
        BuktiTransfer = p.BuktiTransfer,
        TanggalBayar = p.TanggalBayar,
        DibuatPada = p.DibuatPada
    };
}
