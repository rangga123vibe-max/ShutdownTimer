using System;
using System.Diagnostics;

namespace ShutdownTimer.Services
{
    /// <summary>
    /// Kelas ini bertugas menjalankan perintah shutdown/cancel shutdown
    /// Windows menggunakan proses "shutdown.exe" bawaan Windows,
    /// dijalankan secara tersembunyi (tanpa menampilkan jendela CMD).
    /// </summary>
    public static class ShutdownService
    {
        /// <summary>
        /// Menjadwalkan shutdown Windows setelah sejumlah detik tertentu.
        /// </summary>
        /// <param name="delaySeconds">Waktu tunda sebelum shutdown (dalam detik)</param>
        /// <returns>true jika perintah berhasil dijalankan, false jika terjadi error</returns>
        public static bool ScheduleShutdown(int delaySeconds)
        {
            try
            {
                // "shutdown /s /t X" artinya:
                // /s = shutdown (bukan restart)
                // /t X = tunggu X detik dulu sebelum benar-benar shutdown
                RunHiddenCommand($"/s /t {delaySeconds}");
                return true;
            }
            catch (Exception)
            {
                // Kalau gagal (misal karena masalah permission), kita return false
                // supaya UI bisa menampilkan pesan error ke user
                return false;
            }
        }

        /// <summary>
        /// Membatalkan jadwal shutdown yang sudah diset sebelumnya.
        /// </summary>
        public static bool CancelShutdown()
        {
            try
            {
                // "shutdown /a" artinya "abort" -> batalkan shutdown yang sedang dijadwalkan
                RunHiddenCommand("/a");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Method private (hanya bisa dipanggil dari dalam kelas ini sendiri)
        /// untuk menjalankan "shutdown.exe" secara tersembunyi di background.
        /// </summary>
        private static void RunHiddenCommand(string arguments)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "shutdown.exe",
                Arguments = arguments,

                // Ini kunci utamanya: mencegah jendela CMD muncul ke user
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(processInfo);
        }
    }
}