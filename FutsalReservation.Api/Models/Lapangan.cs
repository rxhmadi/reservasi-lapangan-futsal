namespace FutsalReservation.Api.Models;

public class Lapangan
{
    public int Id { get; set; }
    public string Nama { get; set; } = string.Empty;

    // Sintetis, Vinyl, Rumput, dll
    public string Jenis { get; set; } = string.Empty;

    public decimal HargaPerJam { get; set; }
    public string? Deskripsi { get; set; }
    public bool Aktif { get; set; } = true;

    // URL/path gambar utama dan galeri (galeri dipisah baris baru)
    public string? GambarUtama { get; set; }
    public string? Galeri { get; set; }

    public DateTime DibuatPada { get; set; } = DateTime.Now;

    public ICollection<Reservasi> DaftarReservasi { get; set; } = new List<Reservasi>();
}
