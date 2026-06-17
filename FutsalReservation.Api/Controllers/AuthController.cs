using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FutsalReservation.Api.Data;
using FutsalReservation.Api.Dtos;
using FutsalReservation.Api.Models;
using FutsalReservation.Api.Services;

namespace FutsalReservation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _token;

    public AuthController(AppDbContext db, TokenService token)
    {
        _db = db;
        _token = token;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        var email = req.Email.Trim().ToLower();

        bool sudahAda = await _db.Users.AnyAsync(u => u.Email == email);
        if (sudahAda)
            return BadRequest(new { message = "Email sudah terdaftar." });

        var user = new User
        {
            Nama = req.Nama.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = "User"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new AuthResponse
        {
            UserId = user.Id,
            Nama = user.Nama,
            Email = user.Email,
            Role = user.Role,
            Token = _token.BuatToken(user)
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var email = req.Email.Trim().ToLower();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Email atau password salah." });

        return Ok(new AuthResponse
        {
            UserId = user.Id,
            Nama = user.Nama,
            Email = user.Email,
            Role = user.Role,
            Token = _token.BuatToken(user)
        });
    }
}
