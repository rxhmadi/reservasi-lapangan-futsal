# Manual Book — Sistem Reservasi Lapangan Futsal

Dokumen ini menjelaskan cara memasang, menjalankan, dan menggunakan aplikasi.

---

## 1. Gambaran Umum

Aplikasi terdiri dari dua bagian yang berjalan terpisah (sistem terdistribusi):

- **API** (backend) pada `http://localhost:5251`
- **Web** (frontend) pada `http://localhost:5071`

Pengguna mengakses aplikasi melalui browser di alamat frontend. Frontend mengambil dan
mengirim data ke backend melalui API.

---

## 2. Persiapan

1. Pastikan **.NET SDK 8.0** terpasang. Cek dengan:
   ```
   dotnet --version
   ```
2. Pastikan **SQL Server** (Express atau LocalDB) berjalan.
3. Buka file `FutsalReservation.Api/appsettings.json` dan sesuaikan `Server=` pada
   connection string bila nama instance SQL Server Anda berbeda.

---

## 3. Menjalankan Aplikasi

Buka dua jendela terminal.

**Terminal 1 (Backend):**
```
cd FutsalReservation.Api
dotnet run
```
Tunggu hingga muncul `Now listening on: http://localhost:5251`. Pada saat ini database dan
data awal dibuat otomatis.

**Terminal 2 (Frontend):**
```
cd FutsalReservation.Web
dotnet run
```
Buka browser ke `http://localhost:5071`.

---

## 4. Masuk ke Aplikasi

Gunakan salah satu akun berikut:

| Peran | Email             | Password |
|-------|-------------------|----------|
| Admin | admin@futsal.test | admin123 |
| User  | user@futsal.test  | user123  |

Pengguna baru juga dapat menekan **Daftar di sini** pada halaman login untuk membuat akun
(otomatis berperan sebagai User).

---

## 5. Panduan untuk User

### 5.1 Membuat Reservasi
1. Pada menu atas, pilih **Reservasi** lalu **Buat Reservasi**.
2. Pilih lapangan, tanggal, jam mulai, dan jam selesai.
   Perkiraan biaya tampil otomatis di panel sebelah kanan.
3. Tekan **Buat Reservasi**. Anda diarahkan ke halaman detail.

> Jika jam yang dipilih sudah dipesan orang lain, sistem menolak dan meminta Anda memilih
> jam lain.

### 5.2 Membayar
1. Di halaman **Detail Reservasi**, pada kotak **Lakukan Pembayaran**, pilih salah satu metode:
   - **Transfer Bank / E-Wallet**: nomor rekening/akun admin tampil untuk Anda transfer.
     Setelah transfer, **wajib mengunggah bukti transfer** (foto/screenshot) pada kolom yang muncul.
   - **Bayar di Tempat**: sistem membuat **kode booking** yang Anda tunjukkan ke petugas di lokasi
     (tidak perlu unggah bukti).
2. Tekan tombol bayar. Status pembayaran berubah menjadi **Menunggu Verifikasi**.
3. Tunggu admin memverifikasi. Setelah diverifikasi, reservasi menjadi **Dikonfirmasi**.

> Bukti transfer wajib diunggah untuk metode bank/e-wallet; jika belum diunggah, sistem
> menolak pembayaran.

### 5.3 Membatalkan
Pada halaman detail, tekan **Batalkan Reservasi** selama reservasi belum selesai. Akan muncul
dialog konfirmasi sebelum pembatalan dijalankan.

---

## 6. Panduan untuk Admin

### 6.1 Kelola Lapangan
1. Pilih menu **Lapangan**.
2. **Tambah Lapangan** untuk menambah data baru. Pada form, Anda dapat **mengunggah gambar
   utama** dan **beberapa gambar galeri** (JPG/PNG/WEBP, maks 5 MB per file).
3. Tombol **Ubah** dan **Hapus** tersedia pada setiap kartu. Saat mengubah, gambar lama tetap
   dipakai bila Anda tidak mengunggah yang baru.
4. Menghapus akan memunculkan dialog konfirmasi. Lapangan yang sudah memiliki reservasi tidak
   bisa dihapus; cukup nonaktifkan.

### 6.2 Kelola Metode Pembayaran
1. Pilih menu **Metode Bayar** (atau kartu "Metode Pembayaran" di dashboard).
2. **Tambah Metode** untuk menambah rekening bank, e-wallet, atau opsi tunai/bayar di tempat.
   - Untuk bank/e-wallet, isi nomor rekening/akun dan atas nama.
   - Untuk opsi tunai, pilih tipe **Tunai**; pemesan akan mendapat kode booking otomatis.
3. Metode nonaktif tidak akan muncul saat pemesan membayar.

### 6.3 Verifikasi Pembayaran
1. Pilih menu **Pembayaran**. Tabel menampilkan jumlah, metode (lengkap dengan nomor
   rekening), **tanggal bayar**, dan kolom **Bukti**.
2. Klik **Lihat** pada kolom Bukti untuk menampilkan gambar bukti transfer dalam jendela
   popup (tanpa berpindah halaman).
3. Pada pembayaran berstatus **Menunggu Verifikasi**, tekan **Verifikasi**; muncul dialog
   konfirmasi terlebih dulu.
4. Setelah dikonfirmasi, sistem otomatis menandai pembayaran **Lunas** dan reservasi
   **Dikonfirmasi**.

### 6.4 Menyelesaikan Reservasi
Buka detail reservasi yang sudah dikonfirmasi, tekan **Tandai Selesai**.

### 6.5 Dashboard
Menampilkan jumlah lapangan, total reservasi, pembayaran yang perlu diverifikasi, dan total
pendapatan dari pembayaran yang lunas.

---

## 7. Status dalam Sistem

**Status Reservasi**
- `Menunggu` — baru dibuat, belum dikonfirmasi
- `Dikonfirmasi` — pembayaran sudah diverifikasi
- `Selesai` — sudah selesai dimainkan
- `Dibatalkan` — dibatalkan user/admin

**Status Pembayaran**
- `Belum Dibayar` — pemesan belum mengirim pembayaran
- `Menunggu Verifikasi` — sudah dibayar, menunggu admin
- `Lunas` — sudah diverifikasi admin

---

## 8. Pemecahan Masalah

| Masalah | Solusi |
|---------|--------|
| Halaman web menampilkan "Tidak dapat terhubung ke server API" | Pastikan backend (Terminal 1) sedang berjalan di port 5251. |
| Gagal konek database saat API dijalankan | Periksa nama instance pada connection string di `appsettings.json`. |
| Ingin memakai data dari backup | Restore `database/FutsalReservasiDb.bak` melalui SSMS. |
