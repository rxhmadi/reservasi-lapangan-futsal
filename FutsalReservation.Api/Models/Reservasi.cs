namespace FutsalReservation.Api.Models;

public class Reservasi
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int LapanganId { get; set; }
    public Lapangan? Lapangan { get; set; }

    public DateTime Tanggal { get; set; }

    // jam dalam format 24 jam (0 - 23), durasi dihitung dari selisihnya
    public int JamMulai { get; set; }
    public int JamSelesai { get; set; }

    public decimal TotalHarga { get; set; }

    // Menunggu, Dikonfirmasi, Dibatalkan, Selesai
    public string Status { get; set; } = "Menunggu";

    public string? Catatan { get; set; }
    public DateTime DibuatPada { get; set; } = DateTime.Now;

    public Pembayaran? Pembayaran { get; set; }
}
