using System.ComponentModel.DataAnnotations;

namespace FutsalReservation.Web.Models;

public class LapanganVm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nama lapangan wajib diisi")]
    [StringLength(100)]
    [Display(Name = "Nama Lapangan")]
    public string Nama { get; set; } = string.Empty;

    [Required(ErrorMessage = "Jenis lapangan wajib diisi")]
    [StringLength(50)]
    [Display(Name = "Jenis Lapangan")]
    public string Jenis { get; set; } = string.Empty;

    [Range(0, 100000000, ErrorMessage = "Harga tidak valid")]
    [Display(Name = "Harga per Jam")]
    public decimal HargaPerJam { get; set; }

    [StringLength(500)]
    [Display(Name = "Deskripsi")]
    public string? Deskripsi { get; set; }

    [StringLength(400)]
    [Display(Name = "URL Gambar Utama")]
    public string? GambarUtama { get; set; }

    [StringLength(2000)]
    [Display(Name = "Galeri (satu URL per baris)")]
    public string? Galeri { get; set; }

    [Display(Name = "Status Aktif")]
    public bool Aktif { get; set; } = true;

    // gambar utama dengan fallback bila kosong
    public string GambarTampil =>
        string.IsNullOrWhiteSpace(GambarUtama) ? "/img/lapangan/default.svg" : GambarUtama;

    // daftar URL galeri hasil pecahan teks (per baris/koma)
    public List<string> GaleriUrls
    {
        get
        {
            var hasil = new List<string>();
            if (!string.IsNullOrWhiteSpace(GambarUtama))
                hasil.Add(GambarUtama.Trim());

            if (!string.IsNullOrWhiteSpace(Galeri))
            {
                var potongan = Galeri.Split(new[] { '\n', '\r', ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in potongan)
                {
                    var url = p.Trim();
                    if (url.Length > 0 && !hasil.Contains(url))
                        hasil.Add(url);
                }
            }

            if (hasil.Count == 0)
                hasil.Add("/img/lapangan/default.svg");

            return hasil;
        }
    }
}
