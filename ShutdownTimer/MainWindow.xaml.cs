using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using ShutdownTimer.Helpers;
using ShutdownTimer.Services;

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

        public MainWindow()
        {
            InitializeComponent();

            // Set interval timer: setiap 1 detik, event "Tick" akan dipanggil
            _countdownTimer.Interval = TimeSpan.FromSeconds(1);
            _countdownTimer.Tick += CountdownTimer_Tick;
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

            // 3. Tampilkan konfirmasi sebelum benar-benar menjadwalkan shutdown
            string waktuFormatted = $"{hours:D2} jam {minutes:D2} menit {seconds:D2} detik";
            MessageBoxResult confirm = MessageBox.Show(
                $"Laptop akan shutdown otomatis dalam {waktuFormatted}.\nLanjutkan?",
                "Konfirmasi Shutdown",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
                return; // User membatalkan, jangan lanjut

            // 4. Simpan total detik untuk perhitungan progress bar nanti
            _totalSeconds = totalSeconds;
            _remainingSeconds = totalSeconds;

            // 5. Jadwalkan shutdown Windows di level OS.
            //    Kita kasih delay yang SAMA dengan countdown kita,
            //    supaya walaupun aplikasi ditutup paksa, shutdown tetap akan terjadi.
            bool success = ShutdownService.ScheduleShutdown(totalSeconds);

            if (!success)
            {
                MessageBox.Show(
                    "Gagal menjadwalkan shutdown. Coba jalankan aplikasi sebagai Administrator.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 6. Mulai countdown di UI
            _isRunning = true;
            _countdownTimer.Start();

            // 7. Update tampilan: disable input & tombol Start, enable tombol Cancel
            SetInputEnabled(false);
            BtnStart.IsEnabled = false;
            BtnCancel.IsEnabled = true;

            UpdateStatus("Shutdown dijadwalkan", Colors.OrangeRed);
        }

        // ============ TOMBOL CANCEL ============
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // 1. Hentikan countdown di aplikasi
            _countdownTimer.Stop();
            _isRunning = false;

            // 2. Batalkan shutdown yang sudah dijadwalkan di level OS
            bool success = ShutdownService.CancelShutdown();

            if (!success)
            {
                MessageBox.Show(
                    "Gagal membatalkan shutdown. Silakan cek manual lewat Command Prompt (shutdown /a).",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // 3. Reset tampilan ke kondisi awal
            SetInputEnabled(true);
            BtnStart.IsEnabled = true;
            BtnCancel.IsEnabled = false;
            ProgressFill.Width = 0;

            UpdateStatus("Shutdown dibatalkan", Colors.Gray);
        }

        // ============ EVENT: DIPANGGIL SETIAP 1 DETIK ============
        private void CountdownTimer_Tick(object? sender, EventArgs e)
        {
            _remainingSeconds--;

            if (_remainingSeconds <= 0)
            {
                // Waktu habis! Hentikan timer aplikasi.
                // (Shutdown Windows-nya sendiri akan tetap jalan otomatis
                //  karena sudah dijadwalkan lewat ShutdownService.ScheduleShutdown tadi)
                _countdownTimer.Stop();
                _isRunning = false;

                TxtCountdown.Text = "00 : 00 : 00";
                UpdateStatus("Shutdown sedang berlangsung...", Colors.Red);
                return;
            }

            // Update tampilan countdown & progress bar setiap detik
            UpdateCountdownDisplay();
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
    }
}