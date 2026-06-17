using FutsalReservation.Api.Models;

namespace FutsalReservation.Api.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (!db.Users.Any())
        {
            db.Users.Add(new User
            {
                Nama = "Administrator",
                Email = "admin@futsal.test",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "Admin"
            });

            db.Users.Add(new User
            {
                Nama = "Budi Santoso",
                Email = "user@futsal.test",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
                Role = "User"
            });

            db.SaveChanges();
        }

        if (!db.MetodePembayaran.Any())
        {
            db.MetodePembayaran.AddRange(
                new MetodePembayaran { Tipe = "Bank", Nama = "BCA", NomorAkun = "1234567890", AtasNama = "Futsal Arena", Aktif = true },
                new MetodePembayaran { Tipe = "Bank", Nama = "BRI", NomorAkun = "0987654321", AtasNama = "Futsal Arena", Aktif = true },
                new MetodePembayaran { Tipe = "E-Wallet", Nama = "DANA", NomorAkun = "081234567890", AtasNama = "Futsal Arena", Aktif = true },
                new MetodePembayaran { Tipe = "E-Wallet", Nama = "OVO", NomorAkun = "081234567890", AtasNama = "Futsal Arena", Aktif = true },
                new MetodePembayaran
                {
                    Tipe = "Tunai",
                    Nama = "Bayar di Tempat",
                    Instruksi = "Tunjukkan kode booking ke petugas dan lakukan pembayaran tunai di lokasi sebelum bermain.",
                    Aktif = true
                });

            db.SaveChanges();
        }

        if (!db.Lapangan.Any())
        {
            db.Lapangan.AddRange(
                new Lapangan
                {
                    Nama = "Lapangan A",
                    Jenis = "Rumput Sintetis",
                    HargaPerJam = 120000,
                    Deskripsi = "Lapangan indoor ukuran standar dengan rumput sintetis baru.",
                    GambarUtama = "/img/lapangan/court-1.svg",
                    Galeri = "/img/lapangan/court-1.svg\n/img/lapangan/court-2.svg\n/img/lapangan/court-4.svg",
                    Aktif = true
                },
                new Lapangan
                {
                    Nama = "Lapangan B",
                    Jenis = "Vinyl",
                    HargaPerJam = 100000,
                    Deskripsi = "Lapangan vinyl, cocok untuk latihan rutin.",
                    GambarUtama = "/img/lapangan/court-2.svg",
                    Galeri = "/img/lapangan/court-2.svg\n/img/lapangan/court-3.svg",
                    Aktif = true
                },
                new Lapangan
                {
                    Nama = "Lapangan C",
                    Jenis = "Rumput Sintetis",
                    HargaPerJam = 150000,
                    Deskripsi = "Lapangan premium dengan tribun penonton dan lampu untuk malam hari.",
                    GambarUtama = "/img/lapangan/court-3.svg",
                    Galeri = "/img/lapangan/court-3.svg\n/img/lapangan/court-1.svg\n/img/lapangan/court-4.svg",
                    Aktif = true
                });

            db.SaveChanges();
        }
        else
        {
            // backfill gambar untuk data lama yang belum memilikinya
            var contoh = new[] { "/img/lapangan/court-1.svg", "/img/lapangan/court-2.svg", "/img/lapangan/court-3.svg" };
            var tanpaGambar = db.Lapangan.Where(l => l.GambarUtama == null).ToList();
            if (tanpaGambar.Count > 0)
            {
                for (int i = 0; i < tanpaGambar.Count; i++)
                {
                    var img = contoh[i % contoh.Length];
                    tanpaGambar[i].GambarUtama = img;
                    tanpaGambar[i].Galeri = string.Join("\n", contoh);
                }
                db.SaveChanges();
            }
        }
    }
}
