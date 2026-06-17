namespace FutsalReservation.Api.Models;

public class Pembayaran
{
    public int Id { get; set; }

    public int ReservasiId { get; set; }
    public Reservasi? Reservasi { get; set; }

    public decimal Jumlah { get; set; }

    // Transfer Bank, Tunai, E-Wallet
    public string Metode { get; set; } = string.Empty;

    // Belum Dibayar, Menunggu Verifikasi, Lunas
    public string Status { get; set; } = "Belum Dibayar";

    // kode booking untuk pembayaran tunai / bayar di tempat
    public string? KodeBooking { get; set; }

    // path bukti transfer yang diunggah user (untuk metode bank/e-wallet)
    public string? BuktiTransfer { get; set; }

    public DateTime? TanggalBayar { get; set; }
    public DateTime DibuatPada { get; set; } = DateTime.Now;
}
