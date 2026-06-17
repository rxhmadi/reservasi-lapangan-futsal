using System.ComponentModel.DataAnnotations;

namespace FutsalReservation.Api.Dtos;

public class ReservasiRequest
{
    [Required]
    public int LapanganId { get; set; }

    [Required]
    public DateTime Tanggal { get; set; }

    [Range(0, 23)]
    public int JamMulai { get; set; }

    [Range(1, 24)]
    public int JamSelesai { get; set; }

    [StringLength(300)]
    public string? Catatan { get; set; }
}

public class ReservasiResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string NamaUser { get; set; } = string.Empty;
    public int LapanganId { get; set; }
    public string NamaLapangan { get; set; } = string.Empty;
    public DateTime Tanggal { get; set; }
    public int JamMulai { get; set; }
    public int JamSelesai { get; set; }
    public decimal TotalHarga { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Catatan { get; set; }
    public DateTime DibuatPada { get; set; }

    public string StatusPembayaran { get; set; } = "Belum Dibayar";
    public string? MetodePembayaran { get; set; }
    public string? KodeBooking { get; set; }
    public string? BuktiTransfer { get; set; }
}

public class UbahStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
