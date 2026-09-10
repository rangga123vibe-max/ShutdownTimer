using System;
using System.Windows.Forms;
using Application = System.Windows.Forms.Application; // hindari bentrok dengan System.Windows.Application (WPF)

namespace ShutdownTimer.Helpers
{
    /// <summary>
    /// Kelas ini membungkus NotifyIcon (ikon di system tray) milik WinForms,
    /// supaya bisa dipakai dari aplikasi WPF untuk dua keperluan:
    ///   1. Menampilkan notifikasi Toast/Balloon (peringatan sebelum shutdown).
    ///   2. Menjadi ikon di pojok kanan bawah taskbar saat window di-minimize
    ///      (fitur "Minimize to Tray").
    /// Dipisah jadi satu kelas sendiri supaya MainWindow.xaml.cs tidak penuh
    /// dengan detail teknis NotifyIcon.
    /// </summary>
    public class NotificationHelper : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;

        /// <summary>
        /// Dipicu saat user meng-klik/double-klik ikon tray untuk membuka kembali window.
        /// MainWindow akan berlangganan (subscribe) event ini.
        /// </summary>
        public event EventHandler? TrayIconDoubleClicked;

        /// <summary>
        /// Dipicu saat user memilih menu "Keluar" dari klik-kanan ikon tray.
        /// </summary>
        public event EventHandler? ExitRequested;

        public NotificationHelper()
        {
            _notifyIcon = new NotifyIcon
            {
                // Menggunakan ikon default aplikasi yang sedang berjalan (.exe),
                // jadi tidak perlu menyiapkan file .ico terpisah.
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath),
                Text = "Shutdown Timer",
                Visible = false // baru dimunculkan saat window di-minimize (lihat MainWindow)
            };

            // Klik ganda pada ikon tray -> munculkan lagi window utama
            _notifyIcon.DoubleClick += (s, e) => TrayIconDoubleClicked?.Invoke(this, EventArgs.Empty);

            // Menu klik-kanan sederhana: Buka & Keluar
            var menu = new ContextMenuStrip();
            menu.Items.Add("Buka", null, (s, e) => TrayIconDoubleClicked?.Invoke(this, EventArgs.Empty));
            menu.Items.Add("Keluar", null, (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty));
            _notifyIcon.ContextMenuStrip = menu;
        }

        /// <summary>
        /// Menampilkan/menyembunyikan ikon di system tray.
        /// </summary>
        public bool IsVisible
        {
            get => _notifyIcon.Visible;
            set => _notifyIcon.Visible = value;
        }

        /// <summary>
        /// Menampilkan notifikasi Toast (balloon tip) di atas system tray.
        /// </summary>
        /// <param name="title">Judul notifikasi</param>
        /// <param name="message">Isi pesan notifikasi</param>
        /// <param name="durationMs">Berapa lama notifikasi tampil (dalam milidetik)</param>
        public void ShowToast(string title, string message, int durationMs = 5000)
        {
            // NotifyIcon HARUS visible dulu supaya balloon tip bisa muncul,
            // walaupun ikonnya sendiri tidak sedang ditampilkan permanen di tray.
            bool wasVisible = _notifyIcon.Visible;
            _notifyIcon.Visible = true;

            _notifyIcon.BalloonTipTitle = title;
            _notifyIcon.BalloonTipText = message;
            _notifyIcon.BalloonTipIcon = ToolTipIcon.Warning;
            _notifyIcon.ShowBalloonTip(durationMs);

            // Kalau sebelumnya ikon memang tidak ingin ditampilkan permanen
            // (window tidak sedang di-minimize ke tray), sembunyikan lagi
            // setelah durasi notifikasi selesai supaya ikon tidak nyangkut di taskbar.
            if (!wasVisible)
            {
                var hideTimer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(durationMs + 500)
                };
                hideTimer.Tick += (s, e) =>
                {
                    _notifyIcon.Visible = false;
                    hideTimer.Stop();
                };
                hideTimer.Start();
            }
        }

        public void Dispose()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }
    }
}
