using System;

namespace ShutdownTimer.Helpers
{
    /// <summary>
    /// Kelas ini bertugas memvalidasi input waktu dari user
    /// sebelum timer countdown dimulai.
    /// </summary>
    public static class InputValidator
    {
        /// <summary>
        /// Mengecek apakah teks yang dimasukkan adalah angka valid
        /// dalam rentang tertentu (misal jam 0-23, menit/detik 0-59).
        /// </summary>
        /// <param name="input">Teks dari TextBox</param>
        /// <param name="min">Batas minimum yang diperbolehkan</param>
        /// <param name="max">Batas maksimum yang diperbolehkan</param>
        /// <param name="result">Nilai angka hasil parsing jika valid</param>
        /// <returns>true jika valid, false jika tidak</returns>
        public static bool TryParseTimeValue(string input, int min, int max, out int result)
        {
            result = 0;

            // Cek apakah input kosong atau bukan angka sama sekali
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Coba ubah teks jadi angka. Kalau gagal (misal user ketik huruf), langsung return false
            if (!int.TryParse(input, out int value))
                return false;

            // Cek apakah angka tersebut ada di dalam rentang yang diizinkan
            if (value < min || value > max)
                return false;

            result = value;
            return true;
        }

        /// <summary>
        /// Validasi keseluruhan input Jam, Menit, Detik sekaligus.
        /// Mengembalikan pesan error jika ada yang tidak valid, atau string kosong jika semua OK.
        /// </summary>
        public static string ValidateAll(string hoursText, string minutesText, string secondsText,
                                          out int hours, out int minutes, out int seconds)
        {
            hours = minutes = seconds = 0;

            if (!TryParseTimeValue(hoursText, 0, 23, out hours))
                return "Jam harus berupa angka antara 0 - 23.";

            if (!TryParseTimeValue(minutesText, 0, 59, out minutes))
                return "Menit harus berupa angka antara 0 - 59.";

            if (!TryParseTimeValue(secondsText, 0, 59, out seconds))
                return "Detik harus berupa angka antara 0 - 59.";

            // Total waktu tidak boleh nol (0 jam 0 menit 0 detik = tidak masuk akal)
            int totalSeconds = (hours * 3600) + (minutes * 60) + seconds;
            if (totalSeconds <= 0)
                return "Total waktu tidak boleh 0. Masukkan durasi yang valid.";

            return string.Empty; // Artinya semua valid, tidak ada error
        }
    }
}