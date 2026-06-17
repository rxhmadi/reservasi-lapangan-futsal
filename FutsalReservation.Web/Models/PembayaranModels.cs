namespace FutsalReservation.Web.Models;

public class PembayaranVm
{
    public int Id { get; set; }
    public int ReservasiId { get; set; }
    public string NamaLapangan { get; set; } = string.Empty;
    public string NamaUser { get; set; } = string.Empty;
    public decimal Jumlah { get; set; }
    public string Metode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusReservasi { get; set; } = string.Empty;
    public string? KodeBooking { get; set; }
    public string? BuktiTransfer { get; set; }
    public DateTime? TanggalBayar { get; set; }
    public DateTime DibuatPada { get; set; }
}

public class DashboardVm
{
    public bool IsAdmin { get; set; }

    // ringkasan admin
    public int TotalLapangan { get; set; }
    public int TotalReservasi { get; set; }
    public int MenungguKonfirmasi { get; set; }
    public int MenungguVerifikasi { get; set; }
    public decimal TotalPendapatan { get; set; }

    // ringkasan user
    public int ReservasiAktif { get; set; }
    public int MenungguBayar { get; set; }

    public List<ReservasiVm> ReservasiTerbaru { get; set; } = new();
}
