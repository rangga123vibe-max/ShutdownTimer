# ⏻ Shutdown Timer

Aplikasi Windows sederhana untuk menjadwalkan **shutdown otomatis** pada waktu yang kamu tentukan sendiri. Dibuat dengan **C# WPF**, tampilan modern dark mode, dan mudah digunakan.

## ✨ Fitur

- ⏱️ Atur waktu shutdown sendiri (jam, menit, detik)
- ⚡ Preset waktu cepat: 15 menit, 30 menit, 1 jam, 2 jam (tinggal klik)
- 🔀 Multi-aksi: **Shutdown**, **Restart**, **Sleep**, atau **Log Off** — tinggal pilih
- 🔴 Countdown real-time dengan tampilan besar & jelas
- 📊 Progress bar yang menunjukkan sisa waktu
- 🔔 Notifikasi Toast otomatis di sisa 5 menit & 1 menit sebelum aksi dijalankan
- 🗕 Minimize to tray — window masuk ke system tray, countdown tetap jalan di background
- ✅ Konfirmasi sebelum aksi benar-benar dijadwalkan
- ❌ Tombol Cancel untuk membatalkan kapan saja
- 🛡️ Validasi input otomatis (mencegah angka tidak valid)
- 🌙 Tampilan dark mode modern dan minimalis
- 🖼️ Custom app icon di taskbar, title bar, dan file .exe
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
2. Pilih aksi yang diinginkan: **Shutdown**, **Restart**, **Sleep**, atau **Log Off**
3. Masukkan durasi waktu (Jam, Menit, Detik), atau klik salah satu tombol **preset cepat** (15 menit/30 menit/1 jam/2 jam)
4. Klik **START TIMER**
5. Konfirmasi jadwal aksi pada dialog yang muncul
6. Countdown akan berjalan otomatis; kamu akan dapat notifikasi di sisa 5 menit dan 1 menit
7. Minimize window kapan saja — aplikasi masuk ke system tray dan countdown tetap berjalan di background
8. Jika ingin membatalkan, klik **CANCEL SHUTDOWN** kapan saja sebelum waktu habis

## ⚠️ Catatan Penting

- Aksi **Shutdown** dan **Restart** dijadwalkan lewat perintah bawaan Windows (`shutdown.exe`), jadi tetap berjalan walau aplikasi ditutup paksa
- Aksi **Sleep** dan **Log Off** tidak mendukung delay di level OS, sehingga dijalankan langsung oleh aplikasi tepat saat countdown mencapai `00:00:00` (aplikasi harus tetap berjalan sampai waktu itu)
- Pastikan semua pekerjaan sudah disimpan sebelum countdown mencapai `00:00:00`
- Jika muncul error saat menjadwalkan aksi, coba jalankan aplikasi sebagai **Administrator**

## 🛠️ Teknologi

- **C#** & **WPF (.NET 8.0)**
- **DispatcherTimer** untuk countdown real-time
- **Process.Start** untuk menjalankan command shutdown Windows secara tersembunyi (tanpa jendela CMD)
- **P/Invoke `SetSuspendState`** (powrprof.dll) untuk aksi Sleep
- **NotifyIcon (Windows Forms)** untuk notifikasi Toast dan ikon system tray

## 📄 Lisensi

Proyek ini bebas digunakan untuk keperluan pribadi maupun pembelajaran.

---
⚠️ Windows mungkin menampilkan peringatan SmartScreen karena aplikasi belum bersertifikat digital. Klik "More info" → "Run anyway" untuk menjalankan aplikasi.
Dibuat oleh **[rangga123vibe-max](https://github.com/rangga123vibe-max)** — by RanZx
