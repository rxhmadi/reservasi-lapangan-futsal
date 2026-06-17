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
public class LapanganController : ControllerBase
{
    private readonly AppDbContext _db;

    public LapanganController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetSemua([FromQuery] bool hanyaAktif = false)
    {
        var query = _db.Lapangan.AsQueryable();
        if (hanyaAktif)
            query = query.Where(l => l.Aktif);

        var data = await query
            .OrderBy(l => l.Nama)
            .Select(l => MapResponse(l))
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSatu(int id)
    {
        var lapangan = await _db.Lapangan.FindAsync(id);
        if (lapangan == null)
            return NotFound(new { message = "Lapangan tidak ditemukan." });

        return Ok(MapResponse(lapangan));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Tambah(LapanganRequest req)
    {
        var lapangan = new Lapangan
        {
            Nama = req.Nama.Trim(),
            Jenis = req.Jenis.Trim(),
            HargaPerJam = req.HargaPerJam,
            Deskripsi = req.Deskripsi,
            GambarUtama = string.IsNullOrWhiteSpace(req.GambarUtama) ? null : req.GambarUtama.Trim(),
            Galeri = string.IsNullOrWhiteSpace(req.Galeri) ? null : req.Galeri.Trim(),
            Aktif = req.Aktif
        };

        _db.Lapangan.Add(lapangan);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSatu), new { id = lapangan.Id }, MapResponse(lapangan));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Ubah(int id, LapanganRequest req)
    {
        var lapangan = await _db.Lapangan.FindAsync(id);
        if (lapangan == null)
            return NotFound(new { message = "Lapangan tidak ditemukan." });

        lapangan.Nama = req.Nama.Trim();
        lapangan.Jenis = req.Jenis.Trim();
        lapangan.HargaPerJam = req.HargaPerJam;
        lapangan.Deskripsi = req.Deskripsi;
        lapangan.GambarUtama = string.IsNullOrWhiteSpace(req.GambarUtama) ? null : req.GambarUtama.Trim();
        lapangan.Galeri = string.IsNullOrWhiteSpace(req.Galeri) ? null : req.Galeri.Trim();
        lapangan.Aktif = req.Aktif;

        await _db.SaveChangesAsync();
        return Ok(MapResponse(lapangan));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Hapus(int id)
    {
        var lapangan = await _db.Lapangan.FindAsync(id);
        if (lapangan == null)
            return NotFound(new { message = "Lapangan tidak ditemukan." });

        // tolak hapus kalau masih dipakai reservasi, supaya data riwayat aman
        bool dipakai = await _db.Reservasi.AnyAsync(r => r.LapanganId == id);
        if (dipakai)
            return BadRequest(new { message = "Lapangan tidak bisa dihapus karena sudah memiliki data reservasi. Nonaktifkan saja." });

        _db.Lapangan.Remove(lapangan);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static LapanganResponse MapResponse(Lapangan l) => new()
    {
        Id = l.Id,
        Nama = l.Nama,
        Jenis = l.Jenis,
        HargaPerJam = l.HargaPerJam,
        Deskripsi = l.Deskripsi,
        GambarUtama = l.GambarUtama,
        Galeri = l.Galeri,
        Aktif = l.Aktif
    };
}
