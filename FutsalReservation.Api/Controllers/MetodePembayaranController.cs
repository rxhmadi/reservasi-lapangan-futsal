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
public class MetodePembayaranController : ControllerBase
{
    private readonly AppDbContext _db;

    public MetodePembayaranController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetSemua([FromQuery] bool hanyaAktif = false)
    {
        var query = _db.MetodePembayaran.AsQueryable();
        if (hanyaAktif)
            query = query.Where(m => m.Aktif);

        var data = await query
            .OrderBy(m => m.Tipe).ThenBy(m => m.Nama)
            .Select(m => MapResponse(m))
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSatu(int id)
    {
        var metode = await _db.MetodePembayaran.FindAsync(id);
        if (metode == null)
            return NotFound(new { message = "Metode pembayaran tidak ditemukan." });

        return Ok(MapResponse(metode));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Tambah(MetodePembayaranRequest req)
    {
        var metode = new MetodePembayaran
        {
            Tipe = req.Tipe.Trim(),
            Nama = req.Nama.Trim(),
            NomorAkun = Bersih(req.NomorAkun),
            AtasNama = Bersih(req.AtasNama),
            Instruksi = Bersih(req.Instruksi),
            Aktif = req.Aktif
        };

        _db.MetodePembayaran.Add(metode);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetSatu), new { id = metode.Id }, MapResponse(metode));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Ubah(int id, MetodePembayaranRequest req)
    {
        var metode = await _db.MetodePembayaran.FindAsync(id);
        if (metode == null)
            return NotFound(new { message = "Metode pembayaran tidak ditemukan." });

        metode.Tipe = req.Tipe.Trim();
        metode.Nama = req.Nama.Trim();
        metode.NomorAkun = Bersih(req.NomorAkun);
        metode.AtasNama = Bersih(req.AtasNama);
        metode.Instruksi = Bersih(req.Instruksi);
        metode.Aktif = req.Aktif;

        await _db.SaveChangesAsync();
        return Ok(MapResponse(metode));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Hapus(int id)
    {
        var metode = await _db.MetodePembayaran.FindAsync(id);
        if (metode == null)
            return NotFound(new { message = "Metode pembayaran tidak ditemukan." });

        _db.MetodePembayaran.Remove(metode);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static string? Bersih(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static MetodePembayaranResponse MapResponse(MetodePembayaran m) => new()
    {
        Id = m.Id,
        Tipe = m.Tipe,
        Nama = m.Nama,
        NomorAkun = m.NomorAkun,
        AtasNama = m.AtasNama,
        Instruksi = m.Instruksi,
        Aktif = m.Aktif
    };
}
