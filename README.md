# Sistem Reservasi Lapangan Futsal

Aplikasi berbasis **sistem terdistribusi** untuk pemesanan lapangan futsal. Terdiri dari dua
aplikasi terpisah yang berkomunikasi melalui HTTP/JSON:

- **Backend** — ASP.NET Core Web API (menyediakan data & logika bisnis)
- **Frontend** — ASP.NET Core MVC (antarmuka pengguna)
- **Database** — SQL Server (Express / LocalDB)

Autentikasi antar layer menggunakan **JWT (JSON Web Token)**. Frontend login ke API,
menyimpan token di session, lalu menyertakannya pada setiap permintaan ke API.

## Teknologi

| Bagian   | Teknologi                                   |
|----------|---------------------------------------------|
| Backend  | ASP.NET Core 8 Web API, Entity Framework Core 8 |
| Frontend | ASP.NET Core 8 MVC, CSS custom, IBM Plex Sans |
| Database | SQL Server 2022 Express                     |
| Auth     | JWT Bearer + BCrypt (hash password)         |

## Struktur Solusi

```
FutsalReservation.sln
├── FutsalReservation.Api/      Backend Web API
│   ├── Controllers/            Auth, Lapangan, Reservasi, Pembayaran, Dashboard
│   ├── Models/                 Entitas: User, Lapangan, Reservasi, Pembayaran
│   ├── Dtos/                   Objek request/response
│   ├── Data/                   AppDbContext + DbSeeder
│   ├── Services/               TokenService (JWT)
│   └── Migrations/             Migrasi EF Core
├── FutsalReservation.Web/      Frontend MVC
│   ├── Controllers/            Account, Home, Lapangan, Reservasi, Pembayaran
│   ├── Models/                 View model
│   ├── Services/               ApiClient (pemanggil API)
│   ├── Filters/                ButuhLogin, ButuhAdmin
│   ├── Helpers/                IconHelper (SVG), Format (Rupiah/tanggal)
│   ├── Views/                  Tampilan Razor
│   └── wwwroot/css/site.css    Tema putih/pink
└── database/
    ├── FutsalReservasiDb.bak           Backup database (restore via SSMS)
    └── schema_FutsalReservasiDb.sql    Script pembuatan skema
```

## Prasyarat

- .NET SDK 8.0
- SQL Server (Express atau LocalDB) aktif

## Konfigurasi Database

Connection string ada di `FutsalReservation.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=FutsalReservasiDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
}
```

Sesuaikan `Server=` dengan instance SQL Server Anda (misal `(localdb)\\MSSQLLocalDB` atau `.\\SQLEXPRESS`).

Database dibuat **otomatis** saat API pertama kali dijalankan (migrasi + data awal akan
diterapkan sendiri). Tidak perlu restore manual. Jika ingin memakai backup, restore
`database/FutsalReservasiDb.bak` melalui SQL Server Management Studio.

## Cara Menjalankan

Jalankan **dua aplikasi** pada terminal terpisah.

Terminal 1 — Backend API:

```
cd FutsalReservation.Api
dotnet run
```

API aktif di `http://localhost:5251` (dokumentasi Swagger: `http://localhost:5251/swagger`).

Terminal 2 — Frontend MVC:

```
cd FutsalReservation.Web
dotnet run
```

Buka `http://localhost:5071` di browser.

> Catatan: jalankan API lebih dulu, karena frontend mengambil data darinya.

## Akun Demo

| Peran | Email              | Password |
|-------|--------------------|----------|
| Admin | admin@futsal.test  | admin123 |
| User  | user@futsal.test   | user123  |

## Fitur

**Admin**
- Kelola lapangan (tambah, ubah, hapus/nonaktifkan) lengkap dengan gambar & galeri
- Kelola metode pembayaran: rekening bank, e-wallet, dan opsi bayar di tempat
- Melihat seluruh reservasi
- Verifikasi pembayaran (otomatis mengonfirmasi reservasi)
- Menandai reservasi selesai
- Dashboard ringkasan + total pendapatan

**User**
- Mendaftar dan login
- Menelusuri katalog lapangan beserta gambar & galeri
- Membuat reservasi (pilih lapangan, tanggal, jam)
- Memilih metode pembayaran (transfer bank, e-wallet, atau bayar di tempat dengan kode booking)
- Melihat & membatalkan reservasi miliknya

## Alur Transaksi

1. User membuat reservasi → sistem mengecek bentrok jadwal dan menghitung total biaya.
2. Status reservasi: **Menunggu**, pembayaran: **Belum Dibayar**.
3. User memilih metode pembayaran:
   - Transfer bank / e-wallet → menyalin nomor rekening admin lalu konfirmasi.
   - Bayar di tempat → sistem membuat **kode booking** untuk ditunjukkan di lokasi.
   Status pembayaran menjadi **Menunggu Verifikasi**.
4. Admin memverifikasi → pembayaran: **Lunas**, reservasi: **Dikonfirmasi**.
5. Setelah selesai dimainkan, admin menandai reservasi **Selesai**.

Reservasi dapat dibatalkan oleh pemilik atau admin selama belum selesai. Jika dibatalkan,
pembayaran yang belum lunas otomatis dibatalkan juga.

## Dokumentasi

- `docs/Manual_Book.md` — panduan penggunaan aplikasi (user & admin).
- `docs/Penjelasan_Lengkap.md` — penjelasan teknis menyeluruh (arsitektur, API, JWT, dll).
- `docs/Deploy_VPS.md` — panduan deploy ke VPS Ubuntu + Nginx.
