using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FutsalReservation.Api.Data;
using FutsalReservation.Api.Dtos;

namespace FutsalReservation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetSemua()
    {
        var data = await _db.Users
            .OrderByDescending(u => u.Role == "Admin")
            .ThenBy(u => u.Nama)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Nama = u.Nama,
                Email = u.Email,
                Role = u.Role,
                DibuatPada = u.DibuatPada,
                JumlahReservasi = u.DaftarReservasi.Count,
                ReservasiAktif = u.DaftarReservasi.Count(r => r.Status == "Dikonfirmasi"),
                TotalPengeluaran = u.DaftarReservasi
                    .Where(r => r.Pembayaran != null && r.Pembayaran.Status == "Lunas")
                    .Sum(r => (decimal?)r.TotalHarga) ?? 0
            })
            .ToListAsync();

        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSatu(int id)
    {
        var user = await _db.Users
            .Where(u => u.Id == id)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Nama = u.Nama,
                Email = u.Email,
                Role = u.Role,
                DibuatPada = u.DibuatPada,
                JumlahReservasi = u.DaftarReservasi.Count,
                ReservasiAktif = u.DaftarReservasi.Count(r => r.Status == "Dikonfirmasi"),
                TotalPengeluaran = u.DaftarReservasi
                    .Where(r => r.Pembayaran != null && r.Pembayaran.Status == "Lunas")
                    .Sum(r => (decimal?)r.TotalHarga) ?? 0
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound(new { message = "Pengguna tidak ditemukan." });

        return Ok(user);
    }
}
