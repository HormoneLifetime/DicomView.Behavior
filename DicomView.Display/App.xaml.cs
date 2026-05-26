using FellowOakDicom;
using FellowOakDicom.Imaging;
using System.Configuration;
using System.Data;
using System.Windows;

namespace DicomView.Display
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            new DicomSetupBuilder().RegisterServices(s => s.AddFellowOakDicom().AddImageManager<WPFImageManager>()).Build();
        }
    }

}
