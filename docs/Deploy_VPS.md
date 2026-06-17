# Panduan Deploy ke VPS Ubuntu + Nginx

Panduan ini menjelaskan cara men-deploy **Sistem Reservasi Lapangan Futsal** (ASP.NET Core
Web API + MVC + SQL Server) ke VPS Ubuntu yang sudah terpasang **Nginx** dan sudah memiliki
satu website lain. Langkah-langkah di sini **tidak mengganggu** website yang sudah ada
(kita hanya menambah konfigurasi baru).

> Ganti `futsal.domain-anda.com` dengan subdomain/domain Anda, dan ganti semua password
> contoh dengan password Anda sendiri.

---

## 1. Arsitektur deployment

```
Internet (HTTPS)
      │
      ▼
   Nginx  (reverse proxy + SSL)  ── website lama Anda (tetap jalan)
      │
      └── futsal.domain-anda.com  ─────────────┐
                                               ▼
                         Web MVC (Kestrel)  127.0.0.1:5071   ← service: futsal-web
                                               │  HTTP + JSON (internal)
                                               ▼
                         Web API (Kestrel)  127.0.0.1:5251   ← service: futsal-api
                                               │  EF Core
                                               ▼
                         SQL Server          127.0.0.1:1433
```

Poin penting:
- **Hanya aplikasi Web (MVC) yang diekspos publik** lewat Nginx. API cukup berjalan di
  `127.0.0.1` (lokal) karena hanya dipanggil oleh Web dari dalam server. Ini lebih aman dan
  bebas masalah CORS.
- API dan Web berjalan sebagai **systemd service** agar otomatis hidup ulang.
- Database dibuat **otomatis** oleh API saat start pertama (EF Core menjalankan migrasi +
  seed data).

---

## 2. Prasyarat & catatan penting

- VPS Ubuntu **20.04 / 22.04** (disarankan, karena SQL Server resmi mendukung versi ini).
- **RAM minimal 2 GB** — SQL Server membutuhkan ~2 GB. Jika RAM kurang, tambahkan swap
  (lihat Lampiran A) atau gunakan server lebih besar.
- Akses `sudo`, domain/subdomain yang sudah diarahkan (A record) ke IP VPS.
- Nginx sudah berjalan.

---

## 3. Pasang .NET 8 SDK

SDK dipakai untuk mem-build (publish) aplikasi di server.

Ubuntu 22.04:
```bash
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

Ubuntu 24.04 (cukup dari repo bawaan):
```bash
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

Verifikasi:
```bash
dotnet --info
```

---

## 4. Pasang SQL Server (untuk Linux)

```bash
# kunci & repo Microsoft (contoh Ubuntu 22.04)
curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | sudo tee /etc/apt/trusted.gpg.d/microsoft.asc > /dev/null
sudo add-apt-repository "$(curl -fsSL https://packages.microsoft.com/config/ubuntu/22.04/mssql-server-2022.list)"
sudo apt-get update
sudo apt-get install -y mssql-server

# setup awal: pilih edition (Express = gratis), set password 'sa'
sudo /opt/mssql/bin/mssql-conf setup
```

Saat setup, pilih **Express** (gratis) dan tetapkan **password sa** yang kuat (minimal 8
karakter, kombinasi huruf besar, kecil, dan angka/simbol), misalnya `Futsal#2026Kuat`.

Cek status:
```bash
systemctl status mssql-server --no-pager
```

> SQL Server hanya mendengarkan di lokal (port 1433). **Jangan** buka port 1433 ke publik.

(Opsional) pasang alat baris perintah `sqlcmd`:
```bash
sudo apt-get install -y mssql-tools18 unixodbc-dev
```

---

## 5. Ambil kode dari GitHub

```bash
cd ~
git clone https://github.com/rxhmadi/reservasi-lapangan-futsal.git
cd reservasi-lapangan-futsal
```

---

## 6. Publish kedua aplikasi

Build versi rilis ke folder home, lalu salin ke `/var/www`.

```bash
cd ~/reservasi-lapangan-futsal

# build (publish) ke folder sementara di home (tanpa sudo, agar file tidak jadi milik root)
dotnet publish FutsalReservation.Api/FutsalReservation.Api.csproj -c Release -o ~/publish/api
dotnet publish FutsalReservation.Web/FutsalReservation.Web.csproj -c Release -o ~/publish/web

# salin ke lokasi produksi
sudo mkdir -p /var/www/futsal-api /var/www/futsal-web
sudo cp -r ~/publish/api/. /var/www/futsal-api/
sudo cp -r ~/publish/web/. /var/www/futsal-web/

# beri kepemilikan ke www-data (user yang menjalankan service)
sudo chown -R www-data:www-data /var/www/futsal-api /var/www/futsal-web
```

---

## 7. Buat systemd service untuk API

Buat file:
```bash
sudo nano /etc/systemd/system/futsal-api.service
```

Isi (sesuaikan password sa dan kunci JWT):
```ini
[Unit]
Description=Futsal Reservation API
After=network.target mssql-server.service

[Service]
WorkingDirectory=/var/www/futsal-api
ExecStart=/usr/bin/dotnet /var/www/futsal-api/FutsalReservation.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=futsal-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5251
Environment=ConnectionStrings__DefaultConnection=Server=localhost;Database=FutsalReservasiDb;User Id=sa;Password=Futsal#2026Kuat;TrustServerCertificate=True;MultipleActiveResultSets=True
Environment=Jwt__Key=ganti-dengan-kunci-acak-minimal-32-karakter-yang-panjang
Environment=Jwt__Issuer=FutsalReservation.Api
Environment=Jwt__Audience=FutsalReservation.Web

[Install]
WantedBy=multi-user.target
```

> Kunci JWT & password disuntik lewat **environment variable**, sehingga menimpa nilai di
> `appsettings.json`. Jadi nilai di repo tidak dipakai di produksi. Buat kunci acak dengan:
> `openssl rand -base64 48`

---

## 8. Buat systemd service untuk Web

```bash
sudo nano /etc/systemd/system/futsal-web.service
```

Isi:
```ini
[Unit]
Description=Futsal Reservation Web (MVC)
After=network.target futsal-api.service

[Service]
WorkingDirectory=/var/www/futsal-web
ExecStart=/usr/bin/dotnet /var/www/futsal-web/FutsalReservation.Web.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=futsal-web
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5071
Environment=ApiBaseUrl=http://127.0.0.1:5251/

[Install]
WantedBy=multi-user.target
```

Aktifkan kedua service:
```bash
sudo systemctl daemon-reload
sudo systemctl enable --now futsal-api
sudo systemctl enable --now futsal-web

# cek status
systemctl status futsal-api --no-pager
systemctl status futsal-web --no-pager
```

API saat start akan **otomatis membuat database `FutsalReservasiDb`**, menerapkan migrasi,
dan mengisi data awal (admin, user, lapangan, metode bayar). Pantau lognya:
```bash
journalctl -u futsal-api -f
```

Uji lokal di server:
```bash
curl -I http://127.0.0.1:5071        # harus 200/302 dari Web
curl http://127.0.0.1:5251/swagger/index.html -I   # API (lokal)
```

---

## 9. Konfigurasi Nginx (reverse proxy)

Buat server block baru (tidak mengubah site lama):
```bash
sudo nano /etc/nginx/sites-available/futsal
```

Isi:
```nginx
server {
    listen 80;
    server_name futsal.domain-anda.com;

    # penting: izinkan unggah file sampai 10 MB (default nginx hanya 1 MB)
    client_max_body_size 10M;

    location / {
        proxy_pass         http://127.0.0.1:5071;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
}
```

Aktifkan & reload:
```bash
sudo ln -s /etc/nginx/sites-available/futsal /etc/nginx/sites-enabled/
sudo nginx -t          # pastikan "syntax is ok"
sudo systemctl reload nginx
```

Sekarang `http://futsal.domain-anda.com` sudah menampilkan aplikasi.

---

## 10. Aktifkan HTTPS (Let's Encrypt)

Jika `certbot` belum ada:
```bash
sudo apt-get install -y certbot python3-certbot-nginx
```

Terbitkan sertifikat (otomatis mengubah server block ke HTTPS):
```bash
sudo certbot --nginx -d futsal.domain-anda.com
```

Ikuti prompt (email, setuju TOS, pilih redirect HTTP→HTTPS). Certbot memperbarui konfigurasi
Nginx dan mengatur perpanjangan otomatis. Uji:
```bash
sudo certbot renew --dry-run
```

> Catatan: aplikasi berjalan HTTP di belakang Nginx; Nginx yang menangani SSL. Karena
> `ASPNETCORE_URLS` hanya HTTP, fitur HTTPS-redirect internal aplikasi tidak aktif (tidak
> menyebabkan loop). Browser pengguna tetap memakai HTTPS dari Nginx.

---

## 11. Firewall (jika memakai UFW)

```bash
sudo ufw allow 'Nginx Full'   # buka 80 & 443
sudo ufw status
```
Jangan membuka port 1433 (SQL Server) maupun 5071/5251 (cukup lokal).

---

## 12. Akun demo & uji akhir

Buka `https://futsal.domain-anda.com` lalu login:

| Peran | Email | Password |
|---|---|---|
| Admin | admin@futsal.test | admin123 |
| User | user@futsal.test | user123 |

> Untuk keamanan produksi, ganti password akun demo setelah login pertama (atau ubah data
> seed sebelum deploy).

Uji: tambah lapangan (unggah gambar), buat reservasi sebagai user, unggah bukti transfer,
verifikasi sebagai admin. Pastikan unggah gambar berhasil (folder uploads writable).

---

## 13. Memperbarui aplikasi (setelah ada perubahan di GitHub)

```bash
cd ~/reservasi-lapangan-futsal
git pull

dotnet publish FutsalReservation.Api/FutsalReservation.Api.csproj -c Release -o ~/publish/api
dotnet publish FutsalReservation.Web/FutsalReservation.Web.csproj -c Release -o ~/publish/web

sudo cp -r ~/publish/api/. /var/www/futsal-api/
sudo cp -r ~/publish/web/. /var/www/futsal-web/
sudo chown -R www-data:www-data /var/www/futsal-api /var/www/futsal-web

sudo systemctl restart futsal-api futsal-web
```

> `cp` tidak menghapus file yang sudah ada, jadi gambar/bukti yang sudah diunggah di
> `/var/www/futsal-web/wwwroot/uploads` tetap aman saat update.

---

## 14. Troubleshooting

| Gejala | Periksa |
|---|---|
| Web error 502 di browser | `journalctl -u futsal-web -f` dan `journalctl -u futsal-api -f` |
| API gagal start / DB error | Pastikan `systemctl status mssql-server` aktif & password sa benar di service |
| "Tidak dapat terhubung ke server API" | Pastikan `futsal-api` jalan dan `ApiBaseUrl` = `http://127.0.0.1:5251/` |
| Unggah gambar gagal / 413 | `client_max_body_size 10M;` di Nginx + izin tulis folder uploads |
| Perubahan tidak muncul | Ulangi langkah 13 lalu restart service |

Perintah berguna:
```bash
journalctl -u futsal-api -n 100 --no-pager
journalctl -u futsal-web -n 100 --no-pager
sudo systemctl restart futsal-api futsal-web
sudo nginx -t && sudo systemctl reload nginx
```

---

## Lampiran A — Menambah swap (jika RAM 1 GB)

```bash
sudo fallocate -l 2G /swapfile
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile
echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab
free -h
```

## Lampiran B — Akses Swagger dari luar (opsional, untuk demo)

Defaultnya API hanya lokal. Jika dosen perlu melihat Swagger publik, tambahkan subdomain
khusus (mis. `api.futsal.domain-anda.com`) dengan server block yang mem-proxy ke
`http://127.0.0.1:5251`, lalu jalankan `certbot` untuk subdomain itu. Sebaiknya nonaktifkan
lagi setelah demo.

---

*Selesai. Setelah semua langkah, aplikasi berjalan di `https://futsal.domain-anda.com`,
website lama Anda tetap berfungsi seperti semula.*
