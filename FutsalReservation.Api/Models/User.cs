namespace FutsalReservation.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Nama { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // "Admin" atau "User"
    public string Role { get; set; } = "User";

    public DateTime DibuatPada { get; set; } = DateTime.Now;

    public ICollection<Reservasi> DaftarReservasi { get; set; } = new List<Reservasi>();
}
