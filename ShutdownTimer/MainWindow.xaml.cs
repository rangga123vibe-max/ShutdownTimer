using System;
using System.Windows;
using System.Windows.Threading;
using ShutdownTimer.Helpers;
using ShutdownTimer.Services;
// Alias eksplisit karena UseWindowsForms=true membuat "Color" ambigu
// (ada di System.Drawing dan System.Windows.Media). Status dot/teks kita
// pakai warna WPF (Media.Color), bukan warna WinForms (Drawing.Color).
using Color = System.Windows.Media.Color;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using Colors = System.Windows.Media.Colors;
using MessageBox = System.Windows.MessageBox;
// Alias eksplisit karena UseWindowsForms=true membuat "Button" ambigu
// (ada di System.Windows.Controls dan System.Windows.Forms). Tombol yang
// diklik user di XAML selalu WPF Button, bukan WinForms Button.
using Button = System.Windows.Controls.Button;

namespace ShutdownTimer
{
    public partial class MainWindow : Window
    {
        // ============ FIELD / VARIABEL KELAS ============

        // Timer bawaan WPF yang aman dipakai untuk update UI secara berkala
        private readonly DispatcherTimer _countdownTimer = new DispatcherTimer();

        // Menyimpan total detik yang tersisa saat ini
        private int _remainingSeconds;

        // Menyimpan total detik awal (saat user pertama kali menekan Start)
        // Dipakai untuk menghitung persentase progress bar
        private int _totalSeconds;

        // Menandai apakah timer sedang berjalan atau tidak
        private bool _isRunning = false;

        // Menyimpan mode aksi yang dipilih user (Shutdown/Restart/Sleep/LogOff)
        // saat tombol Start ditekan.
        private ActionMode _selectedMode = ActionMode.Shutdown;

        // Menangani notifikasi Toast & ikon system tray (lihat Helpers/NotificationHelper.cs)
        private readonly NotificationHelper _notificationHelper = new NotificationHelper();

        // Menandai notifikasi mana saja yang sudah ditampilkan pada sesi countdown
        // saat ini, supaya masing-masing peringatan (5 menit, 1 menit) hanya
        // muncul SATU KALI, bukan berulang setiap detik selama rentang itu.
        private bool _warned5Min = false;
        private bool _warned1Min = false;

        public MainWindow()
        {
            InitializeComponent();

            // Set interval timer: setiap 1 detik, event "Tick" akan dipanggil
            _countdownTimer.Interval = TimeSpan.FromSeconds(1);
            _countdownTimer.Tick += CountdownTimer_Tick;

            // Pastikan ikon tray & resource notifikasi dibersihkan saat window ditutup,
            // supaya ikonnya tidak "nyangkut" di taskbar setelah aplikasi keluar.
            Closed += (s, e) => _notificationHelper.Dispose();

            // ============ MINIMIZE TO TRAY ============
            // Dipanggil setiap kali status window berubah (Normal/Minimized/Maximized).
            StateChanged += MainWindow_StateChanged;

            // Klik ganda (atau menu "Buka") pada ikon tray -> tampilkan lagi window
            _notificationHelper.TrayIconDoubleClicked += (s, e) => RestoreFromTray();

            // Menu "Keluar" pada ikon tray -> tutup aplikasi sepenuhnya
            _notificationHelper.ExitRequested += (s, e) => Close();
        }

        // ============ EVENT: STATUS WINDOW BERUBAH (MIS. DI-MINIMIZE) ============
        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                // Sembunyikan window sepenuhnya dari taskbar biasa...
                Hide();

                // ...dan tampilkan ikon di system tray sebagai gantinya.
                // Countdown TIDAK berhenti karena DispatcherTimer tetap berjalan
                // di background selama proses aplikasi masih hidup.
                _notificationHelper.IsVisible = true;
            }
        }

        // ============ HELPER: KEMBALIKAN WINDOW DARI SYSTEM TRAY ============
        private void RestoreFromTray()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate(); // pastikan window pindah ke depan (fokus)

            // Ikon tray tidak perlu tampil terus-menerus kalau window sudah dibuka lagi
            _notificationHelper.IsVisible = false;
        }

        // ============ TOMBOL PRESET WAKTU CEPAT ============
        // Dipakai oleh keempat tombol preset (15 menit, 30 menit, 1 jam, 2 jam).
        // Semua tombol memanggil method yang SAMA ini -> event handler dibagi (shared),
        // bedanya cuma dibaca dari properti Tag masing-masing tombol.
        private void PresetButton_Click(object sender, RoutedEventArgs e)
        {
            // Jangan izinkan ganti preset saat timer sedang berjalan
            if (_isRunning) return;

            // Tag tombol berisi teks format "jam:menit:detik", contoh "0:15:0" untuk 15 menit.
            // Kita ambil dari sender (tombol mana yang barusan diklik) lalu di-cast ke Button.
            var button = (Button)sender;
            string tagText = button.Tag?.ToString() ?? "0:0:0";
            string[] parts = tagText.Split(':');

            int hours = int.Parse(parts[0]);
            int minutes = int.Parse(parts[1]);
            int seconds = int.Parse(parts[2]);

            // Isi otomatis ke TextBox jam/menit/detik, format 2 digit (misal "01" bukan "1")
            TxtHours.Text = hours.ToString("D2");
            TxtMinutes.Text = minutes.ToString("D2");
            TxtSeconds.Text = seconds.ToString("D2");
        }

        // ============ TOMBOL START ============
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validasi input dulu sebelum apa pun
            string errorMessage = InputValidator.ValidateAll(
                TxtHours.Text, TxtMinutes.Text, TxtSeconds.Text,
                out int hours, out int minutes, out int seconds);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                MessageBox.Show(errorMessage, "Input Tidak Valid",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Hentikan proses, jangan lanjut ke shutdown
            }

            // 2. Hitung total detik dari jam+menit+detik yang diinput user
            int totalSeconds = (hours * 3600) + (minutes * 60) + seconds;

            // 3. Cari tahu mode aksi mana yang dipilih user lewat RadioButton
            _selectedMode = GetSelectedMode();

            // 4. Tampilkan konfirmasi sebelum benar-benar menjadwalkan aksi
            string waktuFormatted = $"{hours:D2} jam {minutes:D2} menit {seconds:D2} detik";
            string aksiFormatted = GetActionLabel(_selectedMode);
            MessageBoxResult confirm = MessageBox.Show(
                $"Laptop akan {aksiFormatted} otomatis dalam {waktuFormatted}.\nLanjutkan?",
                "Konfirmasi Aksi",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
                return; // User membatalkan, jangan lanjut

            // 5. Simpan total detik untuk perhitungan progress bar nanti
            _totalSeconds = totalSeconds;
            _remainingSeconds = totalSeconds;

            // Reset status notifikasi peringatan setiap kali sesi timer baru dimulai
            _warned5Min = false;
            _warned1Min = false;

            // 6. Jadwalkan aksi di level OS (khusus Shutdown/Restart).
            //    Untuk Sleep/LogOff, ScheduleAction tidak melakukan apa-apa;
            //    aksinya baru benar-benar dijalankan saat countdown mencapai 0
            //    (lihat CountdownTimer_Tick).
            bool success = ShutdownService.ScheduleAction(_selectedMode, totalSeconds);

            if (!success)
            {
                MessageBox.Show(
                    "Gagal menjadwalkan aksi. Coba jalankan aplikasi sebagai Administrator.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 7. Mulai countdown di UI
            _isRunning = true;
            _countdownTimer.Start();

            // 8. Update tampilan: disable input & tombol Start, enable tombol Cancel
            SetInputEnabled(false);
            BtnStart.IsEnabled = false;
            BtnCancel.IsEnabled = true;

            UpdateStatus($"{aksiFormatted} dijadwalkan", Colors.OrangeRed);
        }

        // ============ TOMBOL CANCEL ============
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // 1. Hentikan countdown di aplikasi
            _countdownTimer.Stop();
            _isRunning = false;

            // 2. Batalkan aksi yang sudah dijadwalkan di level OS.
            //    Untuk Sleep/LogOff, tidak ada apa pun yang perlu dibatalkan di OS
            //    (karena keduanya tidak pernah dijadwalkan di level OS),
            //    jadi kita cukup panggil CancelShutdown untuk jaga-jaga saja.
            bool success = ShutdownService.CancelShutdown();

            if (!success)
            {
                MessageBox.Show(
                    "Gagal membatalkan aksi. Silakan cek manual lewat Command Prompt (shutdown /a).",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // 3. Reset tampilan ke kondisi awal
            SetInputEnabled(true);
            BtnStart.IsEnabled = true;
            BtnCancel.IsEnabled = false;
            ProgressFill.Width = 0;

            UpdateStatus("Dibatalkan", Colors.Gray);
        }

        // ============ EVENT: DIPANGGIL SETIAP 1 DETIK ============
        private void CountdownTimer_Tick(object? sender, EventArgs e)
        {
            _remainingSeconds--;

            if (_remainingSeconds <= 0)
            {
                // Waktu habis! Hentikan timer aplikasi.
                _countdownTimer.Stop();
                _isRunning = false;

                TxtCountdown.Text = "00 : 00 : 00";

                // Sleep dan LogOff TIDAK dijadwalkan oleh Windows (lihat ShutdownService),
                // jadi aksinya baru dijalankan sekarang, tepat saat countdown mencapai 0.
                if (_selectedMode == ActionMode.Sleep || _selectedMode == ActionMode.LogOff)
                {
                    ShutdownService.ExecuteImmediateAction(_selectedMode);
                }

                // Untuk Shutdown/Restart, OS sendiri yang akan tetap jalan otomatis
                // karena sudah dijadwalkan lewat ShutdownService.ScheduleAction tadi.
                UpdateStatus($"{GetActionLabel(_selectedMode)} sedang berlangsung...", Colors.Red);
                return;
            }

            // Update tampilan countdown & progress bar setiap detik
            UpdateCountdownDisplay();

            // ============ CEK APAKAH SUDAH WAKTUNYA MENAMPILKAN PERINGATAN ============
            // Ditaruh SETELAH pengecekan _remainingSeconds <= 0 di atas, jadi baris ini
            // hanya dijalankan kalau timer masih berjalan (belum waktunya eksekusi aksi).
            string aksi = GetActionLabel(_selectedMode);

            if (!_warned5Min && _remainingSeconds <= 5 * 60)
            {
                _warned5Min = true; // supaya tidak muncul lagi berkali-kali
                _notificationHelper.ShowToast(
                    "Peringatan Shutdown Timer",
                    $"Laptop akan {aksi} dalam 5 menit lagi. Segera simpan pekerjaan Anda!");
            }
            else if (!_warned1Min && _remainingSeconds <= 1 * 60)
            {
                _warned1Min = true;
                _notificationHelper.ShowToast(
                    "Peringatan Shutdown Timer",
                    $"Laptop akan {aksi} dalam 1 menit lagi!");
            }
        }

        // ============ HELPER: UPDATE TAMPILAN COUNTDOWN ============
        private void UpdateCountdownDisplay()
        {
            TimeSpan time = TimeSpan.FromSeconds(_remainingSeconds);

            // Format jadi "HH : mm : ss", contoh "02 : 35 : 48"
            TxtCountdown.Text = $"{time.Hours:D2} : {time.Minutes:D2} : {time.Seconds:D2}";

            // Hitung persentase progress (0.0 sampai 1.0)
            double progress = _totalSeconds > 0
                ? (double)_remainingSeconds / _totalSeconds
                : 0;

            // Lebar penuh progress bar container adalah lebar Border luar.
            // Kita ambil dari ActualWidth agar responsif terhadap ukuran window.
            double maxWidth = ((FrameworkElement)ProgressFill.Parent).ActualWidth;
            ProgressFill.Width = maxWidth * progress;
        }

        // ============ HELPER: ENABLE/DISABLE INPUT TEXTBOX ============
        private void SetInputEnabled(bool enabled)
        {
            TxtHours.IsEnabled = enabled;
            TxtMinutes.IsEnabled = enabled;
            TxtSeconds.IsEnabled = enabled;
        }

        // ============ HELPER: UPDATE TEKS & WARNA STATUS ============
        private void UpdateStatus(string message, Color color)
        {
            TxtStatus.Text = message;
            TxtStatus.Foreground = new SolidColorBrush(color);
            StatusDot.Fill = new SolidColorBrush(color);
        }

        // ============ HELPER: BACA RADIOBUTTON MANA YANG DIPILIH ============
        private ActionMode GetSelectedMode()
        {
            if (RbRestart.IsChecked == true) return ActionMode.Restart;
            if (RbSleep.IsChecked == true) return ActionMode.Sleep;
            if (RbLogOff.IsChecked == true) return ActionMode.LogOff;

            // Default: Shutdown (juga dipakai kalau entah bagaimana tidak ada yang tercentang)
            return ActionMode.Shutdown;
        }

        // ============ HELPER: UBAH ENUM MODE JADI TEKS UNTUK DITAMPILKAN ============
        private string GetActionLabel(ActionMode mode)
        {
            return mode switch
            {
                ActionMode.Shutdown => "shutdown",
                ActionMode.Restart => "restart",
                ActionMode.Sleep => "sleep",
                ActionMode.LogOff => "log off",
                _ => "shutdown"
            };
        }
    }
}