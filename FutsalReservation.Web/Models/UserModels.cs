namespace FutsalReservation.Web.Models;

// data pengguna untuk panel admin (hasil dari API)
public class UserVm
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
