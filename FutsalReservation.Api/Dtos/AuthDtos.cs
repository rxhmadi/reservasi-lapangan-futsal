using System.ComponentModel.DataAnnotations;

namespace FutsalReservation.Api.Dtos;

public class RegisterRequest
{
    [Required, StringLength(100)]
    public string Nama { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public int UserId { get; set; }
    public string Nama { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

// data pengguna untuk panel admin (tanpa password)
public class UserResponse
{
    public int Id { get; set; }
    public string Nama { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime DibuatPada { get; set; }
    public int JumlahReservasi { get; set; }
    public int ReservasiAktif { get; set; }
    public decimal TotalPengeluaran { get; set; }
}
