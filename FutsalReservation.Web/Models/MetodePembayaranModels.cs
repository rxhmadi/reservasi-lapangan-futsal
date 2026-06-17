using System.ComponentModel.DataAnnotations;

namespace FutsalReservation.Web.Models;

public class MetodePembayaranVm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tipe wajib dipilih")]
    [Display(Name = "Tipe")]
    public string Tipe { get; set; } = "Bank";

    [Required(ErrorMessage = "Nama wajib diisi")]
    [StringLength(80)]
    [Display(Name = "Nama (mis. BCA / DANA)")]
    public string Nama { get; set; } = string.Empty;

    [StringLength(60)]
    [Display(Name = "Nomor Rekening / Akun")]
    public string? NomorAkun { get; set; }

    [StringLength(100)]
    [Display(Name = "Atas Nama")]
    public string? AtasNama { get; set; }

    [StringLength(300)]
    [Display(Name = "Instruksi (opsional)")]
    public string? Instruksi { get; set; }

    [Display(Name = "Aktif")]
    public bool Aktif { get; set; } = true;
}
