using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ShutdownTimer.Services
{
    /// <summary>
    /// Daftar mode aksi yang bisa dijalankan aplikasi ini.
    /// Konsep "satu service, banyak mode" -> satu class ShutdownService
    /// tapi bisa melakukan 4 hal berbeda tergantung mode yang dipilih.
    /// </summary>
    public enum ActionMode
    {
        Shutdown,
        Restart,
        Sleep,
        LogOff
    }

    /// <summary>
    /// Kelas ini bertugas menjalankan perintah shutdown/restart/sleep/logoff
    /// Windows menggunakan proses "shutdown.exe" bawaan Windows (dan API
    /// powrprof.dll khusus untuk Sleep), dijalankan secara tersembunyi
    /// (tanpa menampilkan jendela CMD).
    /// </summary>
    public static class ShutdownService
    {
        // ============ P/INVOKE: PANGGIL FUNGSI WINDOWS API LANGSUNG ============
        // shutdown.exe TIDAK BISA menidurkan (sleep) laptop dengan delay.
        // Satu-satunya cara sleep dari kode adalah lewat fungsi SetSuspendState
        // yang ada di file powrprof.dll milik Windows.
        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);

        /// <summary>
        /// Menjadwalkan aksi (Shutdown/Restart) di level OS setelah sejumlah detik.
        /// Khusus untuk Sleep dan LogOff, TIDAK bisa dijadwalkan dengan delay oleh
        /// Windows, jadi keduanya akan langsung dieksekusi oleh aplikasi sendiri
        /// (lihat ExecuteImmediateAction) begitu countdown di UI mencapai 0.
        /// </summary>
        /// <param name="mode">Aksi apa yang ingin dilakukan</param>
        /// <param name="delaySeconds">Waktu tunda dalam detik (hanya dipakai untuk Shutdown/Restart)</param>
        /// <returns>true jika perintah berhasil dijalankan, false jika terjadi error</returns>
        public static bool ScheduleAction(ActionMode mode, int delaySeconds)
        {
            try
            {
                switch (mode)
                {
                    case ActionMode.Shutdown:
                        // /s = shutdown, /t X = tunggu X detik dulu
                        RunHiddenCommand($"/s /t {delaySeconds}");
                        break;

                    case ActionMode.Restart:
                        // /r = restart, /t X = tunggu X detik dulu
                        RunHiddenCommand($"/r /t {delaySeconds}");
                        break;

                    case ActionMode.Sleep:
                    case ActionMode.LogOff:
                        // Sengaja tidak melakukan apa-apa di sini.
                        // Countdown tetap dihitung oleh aplikasi (DispatcherTimer di MainWindow),
                        // dan aksi sebenarnya baru dipanggil lewat ExecuteImmediateAction()
                        // saat sisa waktu mencapai 0.
                        break;
                }
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
        /// Membatalkan jadwal Shutdown/Restart yang sudah diset sebelumnya di level OS.
        /// Tidak berlaku untuk Sleep/LogOff karena keduanya tidak pernah dijadwalkan
        /// di level OS (lihat penjelasan di ScheduleAction).
        /// </summary>
        public static bool CancelShutdown()
        {
            try
            {
                // "shutdown /a" artinya "abort" -> batalkan shutdown/restart yang sedang dijadwalkan
                RunHiddenCommand("/a");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Menjalankan aksi Sleep atau LogOff secara LANGSUNG (tanpa delay).
        /// Dipanggil oleh MainWindow tepat saat countdown mencapai 0,
        /// khusus untuk mode yang tidak didukung delay oleh Windows.
        /// </summary>
        public static void ExecuteImmediateAction(ActionMode mode)
        {
            switch (mode)
            {
                case ActionMode.Sleep:
                    // hibernate:false -> tidur biasa (bukan hibernate)
                    // forceCritical:true -> paksa tidur walau ada aplikasi yang menolak
                    // disableWakeEvent:true -> perangkat lain tidak bisa membangunkan otomatis
                    SetSuspendState(false, true, true);
                    break;

                case ActionMode.LogOff:
                    // /l = log off user yang sedang aktif (tidak mendukung /t sama sekali)
                    RunHiddenCommand("/l");
                    break;
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