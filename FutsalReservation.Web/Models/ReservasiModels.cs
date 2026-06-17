using System.ComponentModel.DataAnnotations;

namespace FutsalReservation.Web.Models;

// dipakai untuk menampilkan data reservasi (hasil dari API)
public class ReservasiVm
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

// form pembuatan reservasi baru
public class ReservasiFormVm
{
    [Range(1, int.MaxValue, ErrorMessage = "Pilih lapangan terlebih dahulu")]
    [Display(Name = "Lapangan")]
    public int LapanganId { get; set; }

    [Required(ErrorMessage = "Tanggal wajib diisi")]
    [DataType(DataType.Date)]
    [Display(Name = "Tanggal Main")]
    public DateTime Tanggal { get; set; } = DateTime.Today;

    [Range(0, 23, ErrorMessage = "Jam mulai tidak valid")]
    [Display(Name = "Jam Mulai")]
    public int JamMulai { get; set; } = 8;

    [Range(1, 24, ErrorMessage = "Jam selesai tidak valid")]
    [Display(Name = "Jam Selesai")]
    public int JamSelesai { get; set; } = 9;

    [StringLength(300)]
    [Display(Name = "Catatan (opsional)")]
    public string? Catatan { get; set; }

    public List<LapanganVm> DaftarLapangan { get; set; } = new();
}
