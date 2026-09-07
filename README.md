# ⏻ Shutdown Timer

Aplikasi Windows sederhana untuk menjadwalkan **shutdown otomatis** pada waktu yang kamu tentukan sendiri. Dibuat dengan **C# WPF**, tampilan modern dark mode, dan mudah digunakan.

## ✨ Fitur

- ⏱️ Atur waktu shutdown sendiri (jam, menit, detik)
- 🔴 Countdown real-time dengan tampilan besar & jelas
- 📊 Progress bar yang menunjukkan sisa waktu
- ✅ Konfirmasi sebelum shutdown benar-benar dijadwalkan
- ❌ Tombol Cancel untuk membatalkan shutdown kapan saja
- 🛡️ Validasi input otomatis (mencegah angka tidak valid)
- 🌙 Tampilan dark mode modern dan minimalis
- 🚀 Ringan, tidak perlu instalasi tambahan (self-contained)

## 🎯 Manfaat

Aplikasi ini cocok untuk kamu yang:
- Ingin laptop/PC mati otomatis setelah selesai download/render/backup
- Ingin membatasi waktu penggunaan laptop (misalnya waktu belajar/kerja)
- Tidak mau repot mengetik command `shutdown` manual di CMD
- Butuh pengingat visual (countdown) sebelum laptop benar-benar mati

## 📥 Cara Install & Menjalankan

### Opsi 1: Download langsung (paling mudah)

1. Buka bagian **[Releases](../../releases)** di repo ini
2. Download file `ShutdownTimer.exe` dari release terbaru
3. Double-click file tersebut — aplikasi langsung jalan, **tidak perlu install .NET atau software tambahan apapun**

### Opsi 2: Build sendiri dari source code

Jika ingin mengubah/mengembangkan aplikasi ini:

**Persyaratan:**
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (dengan workload ".NET Desktop Development")
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

**Langkah:**

    git clone https://github.com/rangga123vibe-max/ShutdownTimer.git
    cd ShutdownTimer

Buka file `ShutdownTimer.sln` dengan Visual Studio, lalu tekan **F5** untuk menjalankan.

## 🖱️ Cara Menggunakan

1. Buka aplikasi **Shutdown Timer**
2. Masukkan durasi waktu (Jam, Menit, Detik) sebelum laptop shutdown
3. Klik **START TIMER**
4. Konfirmasi jadwal shutdown pada dialog yang muncul
5. Countdown akan berjalan otomatis, laptop akan shutdown saat mencapai `00:00:00`
6. Jika ingin membatalkan, klik **CANCEL SHUTDOWN** kapan saja sebelum waktu habis

## ⚠️ Catatan Penting

- Aplikasi ini menjalankan perintah shutdown bawaan Windows (`shutdown.exe`) di belakang layar
- Pastikan semua pekerjaan sudah disimpan sebelum countdown mencapai `00:00:00`
- Jika muncul error saat menjadwalkan shutdown, coba jalankan aplikasi sebagai **Administrator**

## 🛠️ Teknologi

- **C#** & **WPF (.NET 8.0)**
- **DispatcherTimer** untuk countdown real-time
- **Process.Start** untuk menjalankan command shutdown Windows secara tersembunyi (tanpa jendela CMD)

## 📄 Lisensi

Proyek ini bebas digunakan untuk keperluan pribadi maupun pembelajaran.

---
⚠️ Windows mungkin menampilkan peringatan SmartScreen karena aplikasi belum bersertifikat digital. Klik "More info" → "Run anyway" untuk menjalankan aplikasi.
Dibuat oleh **[rangga123vibe-max](https://github.com/rangga123vibe-max)** — by RanZx
