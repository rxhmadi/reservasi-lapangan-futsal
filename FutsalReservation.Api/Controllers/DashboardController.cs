using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FutsalReservation.Api.Data;

namespace FutsalReservation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    private int UserIdSekarang => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin => User.IsInRole("Admin");

    [HttpGet("ringkasan")]
    public async Task<IActionResult> Ringkasan()
    {
        if (IsAdmin)
        {
            var totalPendapatan = await _db.Pembayaran
                .Where(p => p.Status == "Lunas")
                .SumAsync(p => (decimal?)p.Jumlah) ?? 0;

            return Ok(new
            {
                totalLapangan = await _db.Lapangan.CountAsync(),
                totalReservasi = await _db.Reservasi.CountAsync(),
                menungguKonfirmasi = await _db.Reservasi.CountAsync(r => r.Status == "Menunggu"),
                menungguVerifikasi = await _db.Pembayaran.CountAsync(p => p.Status == "Menunggu Verifikasi"),
                totalPendapatan
            });
        }

        return Ok(new
        {
            totalReservasi = await _db.Reservasi.CountAsync(r => r.UserId == UserIdSekarang),
            reservasiAktif = await _db.Reservasi.CountAsync(r => r.UserId == UserIdSekarang && r.Status == "Dikonfirmasi"),
            menungguBayar = await _db.Pembayaran.CountAsync(p =>
                p.Reservasi!.UserId == UserIdSekarang && p.Status != "Lunas" && p.Reservasi.Status != "Dibatalkan")
        });
    }
}
