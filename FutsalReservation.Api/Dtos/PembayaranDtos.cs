using System.ComponentModel.DataAnnotations;

namespace FutsalReservation.Api.Dtos;

public class BayarRequest
{
    [Required]
    public int ReservasiId { get; set; }

    [Required]
    public int MetodePembayaranId { get; set; }

    [StringLength(300)]
    public string? BuktiTransfer { get; set; }
}

public class PembayaranResponse
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
