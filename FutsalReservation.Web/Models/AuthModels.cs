using System.ComponentModel.DataAnnotations;

namespace FutsalReservation.Web.Models;

public class LoginVm
{
    [Required(ErrorMessage = "Email wajib diisi")]
    [EmailAddress(ErrorMessage = "Format email tidak valid")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password wajib diisi")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;
}

public class RegisterVm
{
    [Required(ErrorMessage = "Nama wajib diisi")]
    [StringLength(100)]
    [Display(Name = "Nama Lengkap")]
    public string Nama { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email wajib diisi")]
    [EmailAddress(ErrorMessage = "Format email tidak valid")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password wajib diisi")]
    [MinLength(6, ErrorMessage = "Password minimal 6 karakter")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ulangi password")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Konfirmasi password tidak cocok")]
    [Display(Name = "Konfirmasi Password")]
    public string KonfirmasiPassword { get; set; } = string.Empty;
}

public class AuthResult
{
    public int UserId { get; set; }
    public string Nama { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
