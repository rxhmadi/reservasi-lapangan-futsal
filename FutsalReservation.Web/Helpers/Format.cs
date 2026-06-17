using System.Globalization;

namespace FutsalReservation.Web.Helpers;

public static class Format
{
    private static readonly CultureInfo Id = new("id-ID");

    public static string Rupiah(decimal nilai) => "Rp " + nilai.ToString("#,##0", Id);

    public static string Jam(int jam) => jam.ToString("00", CultureInfo.InvariantCulture) + ".00";

    public static string Tanggal(DateTime t) => t.ToString("dd MMM yyyy", Id);

    public static string TanggalJam(DateTime t) => t.ToString("dd MMM yyyy, HH:mm", Id);

    // mengembalikan nama kelas badge sesuai status reservasi/pembayaran
    public static string BadgeStatus(string status) => status switch
    {
        "Dikonfirmasi" or "Lunas" or "Selesai" => "badge-hijau",
        "Menunggu" or "Menunggu Verifikasi" => "badge-kuning",
        "Dibatalkan" => "badge-merah",
        "Belum Dibayar" => "badge-abu",
        _ => "badge-abu"
    };
}
