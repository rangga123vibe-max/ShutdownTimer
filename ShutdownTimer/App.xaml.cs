using System.Configuration;
using System.Data;
// Alias eksplisit karena UseWindowsForms=true membuat "Application" menjadi
// ambigu (ada di System.Windows dan System.Windows.Forms). App WPF kita
// harus mewarisi dari System.Windows.Application.
using Application = System.Windows.Application;

namespace ShutdownTimer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
    }

}
