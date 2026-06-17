using System.ComponentModel.DataAnnotations;

namespace FutsalReservation.Api.Dtos;

public class MetodePembayaranRequest
{
    [Required, StringLength(20)]
    public string Tipe { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Nama { get; set; } = string.Empty;

    [StringLength(60)]
    public string? NomorAkun { get; set; }

    [StringLength(100)]
    public string? AtasNama { get; set; }

    [StringLength(300)]
    public string? Instruksi { get; set; }

    public bool Aktif { get; set; } = true;
}

public class MetodePembayaranResponse
{
    public int Id { get; set; }
    public string Tipe { get; set; } = string.Empty;
    public string Nama { get; set; } = string.Empty;
    public string? NomorAkun { get; set; }
    public string? AtasNama { get; set; }
    public string? Instruksi { get; set; }
    public bool Aktif { get; set; }
}
