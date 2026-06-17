# Penjelasan Lengkap Proyek — Sistem Reservasi Lapangan Futsal

Dokumen ini menjelaskan **seluruh bagian** aplikasi secara rinci: konsep, arsitektur,
database, Web API, Swagger, JWT, JSON, frontend MVC, alur kerja, cara menjalankan, cara
demo, sampai daftar kemungkinan pertanyaan dosen beserta jawabannya.

Tujuannya: siapa pun yang membaca dokumen ini bisa **memahami dan menjelaskan** proyek
tanpa harus menebak-nebak.

---

## Daftar Isi
1. Konsep dasar yang wajib dipahami
2. Arsitektur aplikasi
3. Teknologi yang digunakan dan alasannya
4. Struktur folder proyek
5. Database (SQL Server + Entity Framework Core)
6. Backend: ASP.NET Core Web API
7. JWT (autentikasi antar aplikasi)
8. Swagger (dokumentasi & uji API)
9. Frontend: ASP.NET Core MVC
10. Alur kerja end-to-end (request demi request)
11. Cara menjalankan aplikasi
12. Akun demo
13. Cara demo + bukti progres (screenshot)
14. Pemetaan ke Lembar Kemajuan
15. Kemungkinan pertanyaan dosen + jawaban
16. Glosarium istilah

---

## 1. Konsep dasar yang wajib dipahami

**Sistem terdistribusi** adalah sistem yang dibangun dari beberapa aplikasi/komponen yang
berjalan terpisah dan saling berkomunikasi lewat jaringan. Di proyek ini ada **dua aplikasi
terpisah**:
- **Backend (Web API)** — menyimpan data & aturan bisnis.
- **Frontend (Web Client / MVC)** — tampilan yang dipakai pengguna.

Keduanya tidak berbagi memori; mereka bertukar data lewat **HTTP** dalam format **JSON**.
Inilah yang membuat sistem ini "terdistribusi" (memenuhi syarat tugas: *Backend API +
Frontend MVC terpisah*).

Istilah penting (versi singkat, detail di Glosarium):
- **API (Application Programming Interface):** "pintu" yang disediakan backend agar aplikasi
  lain bisa meminta/mengirim data. API kita bergaya **REST** (memakai URL + method HTTP).
- **HTTP method:** `GET` (ambil data), `POST` (buat data), `PUT` (ubah), `DELETE` (hapus).
- **JSON:** format teks untuk bertukar data, contoh `{"nama":"Lapangan A","harga":120000}`.
- **JWT (JSON Web Token):** "tiket" digital hasil login yang dipakai untuk membuktikan
  identitas di setiap permintaan ke API.
- **MVC (Model-View-Controller):** pola penataan kode frontend (Model = data, View =
  tampilan HTML, Controller = pengatur alur).
- **ORM / Entity Framework Core:** alat yang menerjemahkan objek C# menjadi tabel database,
  sehingga kita tidak menulis SQL manual.

---

## 2. Arsitektur aplikasi

```
                      ┌─────────────────────────────┐
   Pengguna  ───────▶ │  FRONTEND  (ASP.NET Core MVC)│   http://localhost:5071
   (browser)          │  - Tampilan HTML/CSS         │
                      │  - Controller + View         │
                      │  - Simpan token di Session   │
                      └──────────────┬──────────────┘
                                     │  HTTP + JSON
                                     │  header: Authorization: Bearer <JWT>
                                     ▼
                      ┌─────────────────────────────┐
                      │  BACKEND  (ASP.NET Core API) │   http://localhost:5251
                      │  - Controller / Endpoint     │
                      │  - Validasi JWT & Role        │
                      │  - Logika bisnis             │
                      └──────────────┬──────────────┘
                                     │  Entity Framework Core
                                     ▼
                      ┌─────────────────────────────┐
                      │   DATABASE (SQL Server)      │   FutsalReservasiDb
                      └─────────────────────────────┘
```

Alur singkat: pengguna membuka frontend → frontend memanggil API → API membaca/menulis
database → API mengembalikan JSON → frontend menampilkannya sebagai HTML.

**Kenapa dipisah?** Supaya backend bisa dipakai banyak client (web, mobile, dll), lebih mudah
diuji (lewat Swagger/Postman), dan sesuai prinsip sistem terdistribusi.

---

## 3. Teknologi yang digunakan dan alasannya

| Bagian | Teknologi | Alasan |
|---|---|---|
| Backend | ASP.NET Core 8 Web API | Sesuai tugas; modern, cepat, dukungan JWT & Swagger bawaan |
| Frontend | ASP.NET Core 8 MVC | Sesuai tugas; render HTML di server, mudah dipadukan dengan API |
| Database | SQL Server (Express) | Sesuai tugas ("SQL Server lokal") |
| Akses data | Entity Framework Core 8 | ORM resmi .NET; migrasi otomatis, tanpa SQL manual |
| Autentikasi | JWT Bearer + BCrypt | Standar industri; BCrypt untuk hash password |
| Dokumentasi API | Swagger (Swashbuckle) | Menguji & mendokumentasikan API tanpa Postman |
| Tampilan | CSS custom + IBM Plex Sans + ikon SVG | Ringan, responsif, tema terang/gelap |

---

## 4. Struktur folder proyek

```
FutsalReservation.sln                 ← file solusi (membungkus 2 proyek)
│
├── FutsalReservation.Api/            ← BACKEND (Web API)
│   ├── Controllers/                  ← endpoint API
│   │   ├── AuthController.cs         ← register & login (terbitkan JWT)
│   │   ├── LapanganController.cs     ← CRUD lapangan
│   │   ├── ReservasiController.cs    ← transaksi reservasi
│   │   ├── PembayaranController.cs   ← pembayaran & verifikasi
│   │   ├── MetodePembayaranController.cs ← kelola rekening/e-wallet/tunai
│   │   └── DashboardController.cs    ← angka ringkasan
│   ├── Models/                       ← ENTITAS (tabel database)
│   │   ├── User.cs, Lapangan.cs, Reservasi.cs, Pembayaran.cs, MetodePembayaran.cs
│   ├── Dtos/                         ← objek request/response (bentuk JSON)
│   ├── Data/
│   │   ├── AppDbContext.cs           ← "jembatan" ke database (EF Core)
│   │   └── DbSeeder.cs               ← isi data awal (admin, lapangan, metode bayar)
│   ├── Services/TokenService.cs      ← pembuat JWT
│   ├── Migrations/                   ← skrip pembentuk tabel (dihasilkan EF)
│   ├── appsettings.json              ← connection string + kunci JWT
│   └── Program.cs                    ← konfigurasi & pipeline API
│
├── FutsalReservation.Web/            ← FRONTEND (MVC)
│   ├── Controllers/                  ← Account, Home, Lapangan, Reservasi, Pembayaran, MetodePembayaran
│   ├── Models/                       ← ViewModel (data untuk View + validasi form)
│   ├── Services/
│   │   ├── ApiClient.cs              ← pemanggil API (HttpClient + JSON + token)
│   │   ├── FileUploadService.cs      ← simpan gambar/bukti ke wwwroot/uploads
│   │   └── SesiKeys.cs               ← nama kunci penyimpanan session
│   ├── Filters/AuthFilters.cs        ← ButuhLogin / ButuhAdmin
│   ├── Helpers/                      ← IconHelper (SVG), Format (Rupiah/tanggal)
│   ├── Views/                        ← halaman Razor (.cshtml)
│   ├── wwwroot/                      ← css, js, gambar
│   ├── appsettings.json              ← alamat API (ApiBaseUrl)
│   └── Program.cs                    ← konfigurasi MVC + session + HttpClient
│
└── database/
    ├── FutsalReservasiDb.bak         ← backup database (restore via SSMS)
    └── schema_FutsalReservasiDb.sql  ← skrip pembuatan tabel
```

Inti yang membuktikan "terdistribusi": **dua proyek terpisah** (`.Api` dan `.Web`) yang
hanya terhubung lewat HTTP.

---

## 5. Database (SQL Server + Entity Framework Core)

Nama database: **FutsalReservasiDb**. Dibuat **otomatis** saat API pertama kali dijalankan
(EF Core menjalankan migrasi, lalu `DbSeeder` mengisi data awal).

### 5.1 Entitas (tabel) dan relasinya

| Tabel | Isi | Kolom penting |
|---|---|---|
| **Users** | akun login | Id, Nama, Email (unik), PasswordHash, Role ("Admin"/"User") |
| **Lapangan** | data utama | Id, Nama, Jenis, HargaPerJam, Deskripsi, GambarUtama, Galeri, Aktif |
| **Reservasi** | transaksi pemesanan | Id, UserId, LapanganId, Tanggal, JamMulai, JamSelesai, TotalHarga, Status |
| **Pembayaran** | transaksi bayar | Id, ReservasiId, Jumlah, Metode, Status, KodeBooking, BuktiTransfer, TanggalBayar |
| **MetodePembayaran** | konfigurasi bayar | Id, Tipe (Bank/E-Wallet/Tunai), Nama, NomorAkun, AtasNama, Aktif |

Relasi:
- **User 1 — N Reservasi** (satu user punya banyak reservasi).
- **Lapangan 1 — N Reservasi** (satu lapangan dipakai banyak reservasi).
- **Reservasi 1 — 1 Pembayaran** (satu reservasi punya satu data pembayaran).

### 5.2 Apa itu Entity Framework Core (EF Core)?

EF Core adalah **ORM**: kita cukup membuat class C# (`Models/`), lalu EF mengubahnya menjadi
tabel SQL. Kita tidak menulis `CREATE TABLE` manual.

- **DbContext** (`Data/AppDbContext.cs`) adalah representasi database di kode. Setiap
  `DbSet<T>` = satu tabel. Contoh: `public DbSet<Lapangan> Lapangan => Set<Lapangan>();`.
- **Migration** = catatan perubahan struktur tabel. Dibuat dengan perintah:
  ```
  dotnet ef migrations add NamaPerubahan
  dotnet ef database update
  ```
  Folder `Migrations/` berisi skrip yang dihasilkan otomatis.
- **Seeding** (`Data/DbSeeder.cs`) mengisi data awal: 1 admin, 1 user, 3 lapangan, dan
  5 metode pembayaran — supaya aplikasi langsung bisa dicoba.

### 5.3 Connection string

Di `FutsalReservation.Api/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=FutsalReservasiDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
}
```
- `Server=.\SQLEXPRESS` → instance SQL Server lokal. Jika di komputer lain memakai LocalDB,
  ganti menjadi `(localdb)\\MSSQLLocalDB`.
- `Trusted_Connection=True` → login ke SQL Server pakai akun Windows (tanpa username/password
  database).

### 5.4 Password tidak disimpan apa adanya

Password user **di-hash** dengan **BCrypt** sebelum disimpan (kolom `PasswordHash`). Jadi
walau database bocor, password asli tidak terbaca. Saat login, sistem mencocokkan password
yang diketik dengan hash memakai `BCrypt.Verify`.

---

## 6. Backend: ASP.NET Core Web API

### 6.1 Pipeline (Program.cs)

`FutsalReservation.Api/Program.cs` melakukan, secara berurutan:
1. Mendaftarkan **DbContext** (koneksi ke SQL Server).
2. Mendaftarkan **TokenService** (pembuat JWT).
3. Mengonfigurasi **autentikasi JWT** (cara memvalidasi token).
4. Mengonfigurasi **CORS** (mengizinkan frontend yang beda alamat memanggil API).
5. Menambahkan **Controllers** dan **Swagger**.
6. Saat start: `db.Database.Migrate()` (buat/aktualkan tabel) lalu `DbSeeder.Seed(db)`.

### 6.2 Entity vs DTO (penting saat ditanya)

- **Entity** (`Models/`) = bentuk data di database.
- **DTO** (`Dtos/`, Data Transfer Object) = bentuk data yang dikirim/diterima lewat JSON.

Kenapa dipisah? Supaya kita tidak membocorkan kolom sensitif (mis. `PasswordHash`) dan agar
respons API rapi. Contoh: `LapanganResponse` adalah versi "aman" dari entity `Lapangan`.

### 6.3 Daftar endpoint API

Semua diawali `http://localhost:5251`. Tanda 🔒 = butuh JWT, 👑 = khusus Admin.

**AuthController** (`/api/auth`)
| Method | URL | Fungsi |
|---|---|---|
| POST | `/api/auth/register` | daftar user baru, balas JWT |
| POST | `/api/auth/login` | login, balas JWT |

**LapanganController** (`/api/lapangan`) 🔒
| Method | URL | Fungsi |
|---|---|---|
| GET | `/api/lapangan` | daftar lapangan (bisa `?hanyaAktif=true`) |
| GET | `/api/lapangan/{id}` | detail satu lapangan |
| POST | `/api/lapangan` 👑 | tambah lapangan |
| PUT | `/api/lapangan/{id}` 👑 | ubah lapangan |
| DELETE | `/api/lapangan/{id}` 👑 | hapus lapangan |

**ReservasiController** (`/api/reservasi`) 🔒
| Method | URL | Fungsi |
|---|---|---|
| GET | `/api/reservasi` | daftar reservasi (admin: semua; user: miliknya) |
| GET | `/api/reservasi/{id}` | detail reservasi |
| POST | `/api/reservasi` | buat reservasi (cek bentrok + hitung total) |
| PUT | `/api/reservasi/{id}/status` | ubah status (mis. Dibatalkan/Selesai) |

**PembayaranController** (`/api/pembayaran`) 🔒
| Method | URL | Fungsi |
|---|---|---|
| GET | `/api/pembayaran` | daftar pembayaran |
| POST | `/api/pembayaran` | kirim pembayaran (pilih metode; tunai → kode booking) |
| PUT | `/api/pembayaran/{id}/verifikasi` 👑 | verifikasi → Lunas + reservasi Dikonfirmasi |

**MetodePembayaranController** (`/api/metodepembayaran`) 🔒
| Method | URL | Fungsi |
|---|---|---|
| GET | `/api/metodepembayaran` | daftar metode (bisa `?hanyaAktif=true`) |
| POST/PUT/DELETE | `/api/metodepembayaran/{id}` 👑 | kelola metode |

**DashboardController** (`/api/dashboard/ringkasan`) 🔒 — angka ringkasan untuk dashboard.

### 6.4 Contoh isi sebuah controller (pola yang dipakai)

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]                         // wajib login (punya JWT valid)
public class LapanganController : ControllerBase
{
    private readonly AppDbContext _db;
    public LapanganController(AppDbContext db) { _db = db; }   // DI: database "disuntikkan"

    [HttpGet]
    public async Task<IActionResult> GetSemua() => Ok(await _db.Lapangan...);

    [HttpPost]
    [Authorize(Roles = "Admin")]    // hanya admin
    public async Task<IActionResult> Tambah(LapanganRequest req) { ... }
}
```
- `[ApiController]` + `[Route("api/[controller]")]` → URL otomatis `/api/lapangan`.
- `[Authorize]` → endpoint hanya bisa diakses jika request membawa JWT valid.
- `[Authorize(Roles="Admin")]` → tambahan: hanya user ber-role Admin.
- **Dependency Injection (DI):** `AppDbContext` dan service lain otomatis "disuntikkan"
  lewat constructor — tidak dibuat manual.

### 6.5 Aturan bisnis penting (sering ditanya)

- **Cek bentrok jadwal:** saat buat reservasi, sistem menolak jika ada reservasi lain pada
  lapangan & tanggal sama dengan jam yang tumpang tindih (kecuali yang berstatus Dibatalkan).
- **Hitung total otomatis:** `TotalHarga = (JamSelesai - JamMulai) × HargaPerJam`.
- **Kode booking:** dibuat hanya untuk metode bertipe **Tunai** (format `BK-0001-XXXX`).
- **Wajib bukti transfer:** untuk metode **Bank/E-Wallet**, user wajib mengunggah bukti
  transfer; API menolak pembayaran tanpa bukti. Metode Tunai tidak memerlukan bukti.
- **Tanggal bayar:** dicatat saat user mengirim pembayaran (bukan saat verifikasi).
- **Metode tersimpan lengkap:** saat membayar via transfer, kolom `Metode` menyimpan snapshot
  `Nama · NomorRekening · a.n. AtasNama` sehingga riwayat tetap akurat meski rekening diubah.
- **Pembatalan:** jika reservasi dibatalkan dan pembayaran belum lunas, pembayaran direset
  agar tidak ikut antrean verifikasi.

---

## 7. JWT (autentikasi antar aplikasi)

### 7.1 Masalah yang dipecahkan JWT

API kita "stateless" (tidak mengingat siapa yang sedang login). Maka tiap permintaan harus
membuktikan identitas. Caranya: setelah login, API memberi **token JWT**. Token ini
dilampirkan di setiap permintaan berikutnya, di header:
```
Authorization: Bearer <token>
```

### 7.2 Bentuk JWT

JWT terdiri dari 3 bagian dipisah titik: `header.payload.signature`.
- **Payload** berisi **claims** (info user): di proyek kita ada Id, Nama, Email, dan **Role**.
- **Signature** = "segel" yang dibuat dengan **kunci rahasia** (di `appsettings.json` →
  `Jwt:Key`). Kalau token diubah, segel tidak cocok → ditolak.

> Penting: JWT hanya **ditandatangani**, bukan dienkripsi. Isinya bisa dibaca (mis. di
> jwt.io), tapi tidak bisa dipalsukan tanpa kunci rahasia.

### 7.3 Cara membuat token (TokenService)

`Services/TokenService.cs` membuat token saat register/login:
```csharp
var claims = new List<Claim> {
    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new(ClaimTypes.Name, user.Nama),
    new(ClaimTypes.Email, user.Email),
    new(ClaimTypes.Role, user.Role)        // ← dipakai untuk [Authorize(Roles="Admin")]
};
var token = new JwtSecurityToken(issuer, audience, claims,
                                 expires: DateTime.Now.AddHours(8), signingCredentials);
```
Token berlaku **8 jam**.

### 7.4 Cara API memvalidasi token

Di `Program.cs`, `AddJwtBearer(...)` memeriksa: issuer, audience, masa berlaku, dan tanda
tangan (kunci). Jika valid, API tahu siapa user-nya dan role-nya, lalu `[Authorize]`
mengizinkan/menolak.

### 7.5 Bagaimana frontend memakai JWT (poin "JWT Web Client")

1. User login di MVC → `AccountController` memanggil `POST /api/auth/login`.
2. API balas JSON berisi `token`.
3. MVC menyimpan token di **Session** (lihat `SesiKeys.Token`).
4. Setiap kali MVC memanggil API, `ApiClient` menambahkan header `Authorization: Bearer
   <token>` secara otomatis.
5. Saat logout, session dihapus → token hilang.

---

## 8. Swagger (dokumentasi & uji API)

**Swagger** adalah halaman web otomatis untuk melihat dan **mencoba** semua endpoint API
tanpa perlu Postman.

- Akses: jalankan API, buka **`http://localhost:5251/swagger`**.
- Setiap endpoint bisa dibuka, diisi parameter, lalu **Try it out → Execute**.

**Cara uji endpoint yang butuh JWT di Swagger:**
1. Buka `POST /api/auth/login`, klik *Try it out*, isi:
   ```json
   { "email": "admin@futsal.test", "password": "admin123" }
   ```
   Execute → lihat respons, **salin nilai `token`**.
2. Klik tombol **Authorize** (gembok) di kanan atas Swagger, ketik token lalu Authorize.
3. Sekarang endpoint ber-🔒 (mis. `GET /api/lapangan`) bisa dicoba dan akan berhasil.

Swagger sudah dikonfigurasi mendukung tombol Authorize (lihat `AddSwaggerGen` di
`Program.cs`). Ini juga **bukti progres** "JWT saat akses TOKEN".

---

## 9. Frontend: ASP.NET Core MVC

### 9.1 Pipeline (Program.cs)

`FutsalReservation.Web/Program.cs`:
- `AddControllersWithViews()` → mengaktifkan MVC.
- `AddSession(...)` → menyimpan token & info user antar-halaman.
- `AddHttpClient<ApiClient>(...)` → membuat `ApiClient` dengan alamat dasar API
  (`ApiBaseUrl` di appsettings = `http://localhost:5251/`).
- Routing default: `{controller=Home}/{action=Index}/{id?}`.

### 9.2 ApiClient — inti "konsumsi API" + "JSON serialize/deserialize"

`Services/ApiClient.cs` membungkus `HttpClient`:
- Menambahkan header `Authorization: Bearer <token>` dari session (kalau ada).
- `PostAsync<T>` memakai `PostAsJsonAsync` → objek C# **di-serialize** menjadi JSON.
- Respons dibaca dengan `ReadFromJsonAsync<T>` → JSON **di-deserialize** menjadi objek C#.
- Hasil dibungkus `HasilApi<T>` berisi: `Sukses`, `Data`, `Pesan` (pesan error dari API),
  sehingga controller MVC tahu sukses/gagal dan bisa menampilkan toast.

### 9.3 Autentikasi di sisi MVC (session + filter)

MVC tidak memakai cookie auth bawaan, tetapi **session**:
- `SesiKeys.cs` menyimpan nama kunci: Token, UserId, Nama, Role.
- `Filters/AuthFilters.cs`:
  - `[ButuhLogin]` → jika belum ada token di session, lempar ke halaman Login.
  - `[ButuhAdmin]` → selain harus login, role harus "Admin"; jika tidak, ditolak.
- Controller diberi atribut ini. Contoh: `LapanganController` aksi Create/Edit/Delete pakai
  `[ButuhAdmin]`, sedangkan melihat katalog cukup `[ButuhLogin]`.

### 9.4 Controller & View MVC

| Controller | Tugas | View utama |
|---|---|---|
| AccountController | login, register, logout | Login, Register |
| HomeController | dashboard + ringkasan | Index |
| LapanganController | katalog & kelola lapangan | Index, Detail, Create, Edit |
| ReservasiController | buat & kelola reservasi | Index, Create, Detail |
| PembayaranController | kirim & verifikasi pembayaran | Index |
| MetodePembayaranController | kelola rekening/e-wallet/tunai | Index, Create, Edit |

### 9.5 Tampilan (View + Layout)

- `Views/Shared/_Layout.cshtml` = kerangka semua halaman: **sidebar** (desktop), **header**
  berisi judul halaman + tombol tema, dan **bottom navigation** (mobile).
- `Views/Shared/_LayoutAuth.cshtml` = kerangka khusus halaman login/register.
- `Helpers/IconHelper.cs` = ikon SVG (gaya Heroicons) dipanggil via `@Html.Ikon("nama")`.
- `Helpers/Format.cs` = format Rupiah, tanggal, jam, dan warna badge status.
- `wwwroot/css/site.css` = seluruh gaya (tema terang/gelap memakai CSS variables;
  pilihan tema disimpan di `localStorage`).
- Validasi form sisi-klien memakai jQuery Validation (di `_ValidationScriptsPartial`).

### 9.6 Unggah file

- `Services/FileUploadService.cs` menyimpan gambar yang diunggah ke
  `wwwroot/uploads/<subfolder>` (mis. `lapangan`, `bukti`), memvalidasi tipe (JPG/PNG/WEBP/GIF)
  dan ukuran (maks 5 MB), lalu mengembalikan **path relatif** (mis. `/uploads/bukti/abc.jpg`).
- **Gambar lapangan** (admin): form Tambah/Ubah mengirim `IFormFile` (gambar utama + galeri).
  Controller menyimpan file, lalu mengirim path-nya ke API sebagai `GambarUtama`/`Galeri`.
- **Bukti transfer** (user): form pembayaran mengirim `IFormFile buktiTransfer`. Controller
  menyimpan file lalu mengirim path-nya ke API. Form memakai `enctype="multipart/form-data"`.
- File disimpan di sisi frontend (wwwroot) sehingga path-nya konsisten dengan gambar lain dan
  langsung bisa ditampilkan via `<img src="...">`.

### 9.7 Komponen UI (tanpa alert bawaan)

- `wwwroot/js/site.js` berisi: **toast** (notifikasi sukses/gagal), **toggle tema**,
  **modal konfirmasi**, dan **lightbox gambar**.
- **Modal konfirmasi:** form yang butuh konfirmasi memakai atribut `data-confirm="pesan"`
  (opsional `data-confirm-ok` dan `data-confirm-variant="danger"`). JS mencegat submit,
  menampilkan dialog, dan baru mengirim form setelah pengguna menekan tombol konfirmasi —
  menggantikan `confirm()` bawaan browser.
- **Lightbox gambar:** elemen dengan atribut `data-img="url"` (mis. tautan "Lihat bukti")
  membuka gambar dalam jendela popup, bukan berpindah halaman.

---

## 10. Alur kerja end-to-end (request demi request)

### 10.1 Login
1. User membuka `http://localhost:5071/Account/Login`, isi email & password.
2. `AccountController.Login` memanggil `POST http://localhost:5251/api/auth/login`.
3. API mencocokkan password (BCrypt), lalu membuat **JWT** dan mengembalikannya sebagai JSON.
4. MVC menyimpan token + nama + role di **session**, lalu mengarahkan ke Dashboard.

### 10.2 Membuat reservasi (modul transaksi)
1. User membuka **Reservasi → Buat Reservasi**, memilih lapangan, tanggal, jam.
2. MVC `POST /api/reservasi` (membawa JWT).
3. API: cek lapangan aktif → **cek bentrok jadwal** → **hitung total** → simpan Reservasi
   (status "Menunggu") + membuat record Pembayaran (status "Belum Dibayar").
4. MVC mengarahkan ke halaman **Detail Reservasi**.

### 10.3 Membayar
1. Di Detail Reservasi, user memilih **metode pembayaran** (diambil dari
   `GET /api/metodepembayaran?hanyaAktif=true`).
2. Untuk **Bank/E-Wallet**, user mengunggah **bukti transfer** (wajib); frontend menyimpan
   file lalu mengirim path-nya. MVC `POST /api/pembayaran` dengan `metodePembayaranId` dan
   `buktiTransfer`.
3. API: set metode (lengkap dengan nomor rekening), catat **tanggal bayar**, status →
   "Menunggu Verifikasi". Jika tipe **Tunai**, dibuat **kode booking** dan bukti tidak wajib.

### 10.4 Verifikasi (admin)
1. Admin membuka menu **Pembayaran**, menekan **Verifikasi**.
2. MVC `PUT /api/pembayaran/{id}/verifikasi`.
3. API: pembayaran → "Lunas", reservasi → "Dikonfirmasi".

### 10.5 Status yang mungkin
- Reservasi: **Menunggu → Dikonfirmasi → Selesai** (atau **Dibatalkan**).
- Pembayaran: **Belum Dibayar → Menunggu Verifikasi → Lunas**.

---

## 11. Cara menjalankan aplikasi

Prasyarat: **.NET SDK 8** dan **SQL Server** (Express/LocalDB) aktif.

Buka **dua terminal** (jalankan API lebih dulu):

Terminal 1 — Backend:
```
cd FutsalReservation.Api
dotnet run
```
Tunggu `Now listening on: http://localhost:5251`. Database & data awal dibuat otomatis.

Terminal 2 — Frontend:
```
cd FutsalReservation.Web
dotnet run
```
Buka `http://localhost:5071`.

> Jika nama instance SQL Server berbeda, sesuaikan `Server=` pada
> `FutsalReservation.Api/appsettings.json`.

---

## 12. Akun demo

| Peran | Email | Password |
|---|---|---|
| Admin | admin@futsal.test | admin123 |
| User | user@futsal.test | user123 |

Admin: kelola lapangan, metode bayar, verifikasi pembayaran. User: pesan lapangan & bayar.

---

## 13. Cara demo + bukti progres (screenshot)

Lembar kemajuan meminta screenshot. Berikut cara mendapatkannya:

1. **Web API & Client jalan di Swagger/Postman:** buka `http://localhost:5251/swagger`
   → screenshot daftar endpoint.
2. **Halaman Login:** buka `http://localhost:5071/Account/Login` → screenshot.
3. **Halaman Data Utama:** login admin → menu **Lapangan** (katalog) → screenshot.
4. **JWT saat akses TOKEN:** di Swagger jalankan `POST /api/auth/login` → screenshot respons
   yang menampilkan field `token`. (Opsional: tunjukkan tombol Authorize.)
5. **Proses CRUD:** di menu Lapangan/Metode Bayar, lakukan Tambah atau Ubah → screenshot
   form dan hasilnya di daftar.

Tips demo presentasi: login admin → tambah lapangan → logout → login user → buat reservasi →
bayar (coba "Bayar di Tempat" untuk memperlihatkan kode booking) → login admin → verifikasi.

---

## 14. Pemetaan ke Lembar Kemajuan

| Komponen di lembar | Status | Letak di proyek |
|---|---|---|
| Database | Sudah | SQL Server + EF Core + Migrations |
| Web API | Sudah | `FutsalReservation.Api` |
| CRUD API | Sudah | Lapangan & MetodePembayaran (CRUD penuh) |
| Web Client | Sudah | `FutsalReservation.Web` (MVC) |
| Konsumsi API | Sudah | `Services/ApiClient.cs` |
| JSON Serialize/Deserialize | Sudah | `PostAsJsonAsync` / `ReadFromJsonAsync` |
| JWT Login | Sudah | `AuthController` + `TokenService` |
| JWT Web Client | Sudah | token di session + header Bearer |
| Integrasi API & Client | Sudah | seluruh data MVC lewat API |

Entitas utama: User, Lapangan, Reservasi, Pembayaran, MetodePembayaran.
URL API `http://localhost:5251` · URL Client `http://localhost:5071`.

---

## 15. Kemungkinan pertanyaan dosen + jawaban

**T: Apa yang membuat sistem ini "terdistribusi"?**
J: Ada dua aplikasi terpisah (Web API dan Web MVC) yang berjalan sendiri-sendiri dan
berkomunikasi lewat HTTP/JSON. Frontend tidak mengakses database langsung — semua lewat API.

**T: Kenapa pakai DTO, tidak langsung entity?**
J: Agar data sensitif (mis. PasswordHash) tidak ikut terkirim, respons lebih rapi, dan
struktur database tidak "bocor" ke luar.

**T: Bagaimana keamanan login?**
J: Password di-hash dengan BCrypt (tidak disimpan asli). Setelah login, identitas dibawa
lewat JWT yang ditandatangani kunci rahasia; token yang diubah otomatis ditolak.

**T: Apa beda Authentication dan Authorization di sini?**
J: Authentication = membuktikan "siapa kamu" (validasi JWT). Authorization = "boleh apa"
(role Admin lewat `[Authorize(Roles="Admin")]`).

**T: Apa fungsi Swagger?**
J: Dokumentasi otomatis sekaligus alat uji endpoint API tanpa Postman, termasuk uji endpoint
ber-JWT lewat tombol Authorize.

**T: Bagaimana frontend menyimpan token?**
J: Di Session server. Setiap permintaan ke API, `ApiClient` menambahkan header
`Authorization: Bearer <token>`.

**T: Bagaimana mencegah dua orang memesan jam yang sama?**
J: Saat membuat reservasi, API mengecek bentrok jadwal (lapangan + tanggal + jam tumpang
tindih, status bukan Dibatalkan) dan menolak jika bentrok.

**T: Bagaimana total harga dihitung?**
J: `(JamSelesai - JamMulai) × HargaPerJam`, dihitung di server (API), bukan di frontend,
supaya tidak bisa dimanipulasi dari browser.

**T: Apa itu kode booking?**
J: Kode untuk pembayaran "bayar di tempat" (tipe Tunai), ditunjukkan saat datang ke lokasi.
Dibuat otomatis oleh API saat user memilih metode tunai.

**T: Migrasi database itu apa?**
J: Catatan perubahan struktur tabel yang dihasilkan EF Core. `database update`
menerapkannya. Membuat skema database bisa diulang di komputer lain.

**T: Kalau API mati, apa yang terjadi?**
J: Frontend menampilkan pesan "tidak dapat terhubung ke server API" (ditangani di
`ApiClient`), karena frontend bergantung pada API.

**T: Bagaimana fitur unggah file bekerja?**
J: Frontend (MVC) menerima file lewat form `multipart/form-data`, menyimpannya ke
`wwwroot/uploads/` via `FileUploadService` (validasi tipe & ukuran maks 5 MB), lalu mengirim
**path** file ke API untuk disimpan di database. Gambar lapangan dan bukti transfer memakai
mekanisme yang sama.

**T: Kenapa user transfer wajib unggah bukti?**
J: Agar admin punya dasar untuk memverifikasi pembayaran. API menolak pembayaran bank/e-wallet
tanpa bukti. Metode tunai tidak wajib karena pembayaran dilakukan langsung di lokasi (dengan
kode booking).

**T: Apakah memakai alert/confirm bawaan browser?**
J: Tidak. Konfirmasi (hapus, batalkan, verifikasi) memakai **modal dialog** sendiri, dan
bukti transfer dibuka dalam **lightbox** (popup gambar), bukan pindah halaman.

**T: Bisakah diakses dari HP?**
J: Tampilan responsif (sidebar di desktop, bottom navigation di mobile). Secara jaringan,
bisa diakses dari perangkat lain jika alamat IP/port dibuka.

---

## 16. Glosarium istilah

- **API:** antarmuka agar aplikasi lain bisa minta/kirim data.
- **REST:** gaya API yang memakai URL + HTTP method (GET/POST/PUT/DELETE).
- **Endpoint:** satu alamat API tertentu, mis. `/api/lapangan`.
- **JSON:** format teks pertukaran data.
- **Serialize / Deserialize:** ubah objek → JSON / JSON → objek.
- **JWT:** token identitas hasil login, ditandatangani kunci rahasia.
- **Claim:** potongan info di dalam JWT (Id, Role, dll).
- **Bearer token:** cara mengirim JWT di header `Authorization`.
- **Hashing (BCrypt):** mengubah password jadi kode tak-balik untuk disimpan aman.
- **ORM / EF Core:** menerjemahkan objek C# ↔ tabel database.
- **DbContext:** objek penghubung kode ke database.
- **Migration:** skrip perubahan struktur database.
- **DTO:** objek khusus untuk bentuk data yang dikirim/diterima API.
- **MVC:** pola Model-View-Controller untuk frontend.
- **Razor (.cshtml):** template HTML + C# untuk membuat View.
- **Session:** penyimpanan sementara di server per pengguna (di sini menyimpan token).
- **CORS:** izin agar frontend beda alamat boleh memanggil API.
- **Dependency Injection (DI):** komponen "disuntikkan" otomatis lewat constructor.
- **Swagger:** dokumentasi + penguji API berbasis web.
- **Middleware/pipeline:** rangkaian langkah pemrosesan setiap request.

---

*Dokumen ini menjelaskan kondisi aplikasi apa adanya. Jika ada bagian kode yang ingin
ditelusuri lebih dalam, mulailah dari `Program.cs` di masing-masing proyek, lalu ikuti alur
ke Controller → Service → Database.*
