namespace FutsalReservation.Api.Models;

public class MetodePembayaran
{
    public int Id { get; set; }

    // "Bank", "E-Wallet", atau "Tunai"
    public string Tipe { get; set; } = string.Empty;

    // nama penyedia, mis. BCA / DANA / Bayar di Tempat
    public string Nama { get; set; } = string.Empty;

    // nomor rekening atau nomor akun e-wallet (kosong untuk tunai)
    public string? NomorAkun { get; set; }

    public string? AtasNama { get; set; }
    public string? Instruksi { get; set; }

    public bool Aktif { get; set; } = true;
    public DateTime DibuatPada { get; set; } = DateTime.Now;
}
