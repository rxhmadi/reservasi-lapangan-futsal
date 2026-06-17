using System.ComponentModel.DataAnnotations;

namespace FutsalReservation.Api.Dtos;

public class LapanganRequest
{
    [Required, StringLength(100)]
    public string Nama { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Jenis { get; set; } = string.Empty;

    [Range(0, 100000000)]
    public decimal HargaPerJam { get; set; }

    [StringLength(500)]
    public string? Deskripsi { get; set; }

    [StringLength(400)]
    public string? GambarUtama { get; set; }

    [StringLength(2000)]
    public string? Galeri { get; set; }

    public bool Aktif { get; set; } = true;
}

public class LapanganResponse
{
    public int Id { get; set; }
    public string Nama { get; set; } = string.Empty;
    public string Jenis { get; set; } = string.Empty;
    public decimal HargaPerJam { get; set; }
    public string? Deskripsi { get; set; }
    public string? GambarUtama { get; set; }
    public string? Galeri { get; set; }
    public bool Aktif { get; set; }
}
