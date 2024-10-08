using ContosoDeviceManager;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Windows;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;

namespace NotesWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public ContosoCamera selectedCamera = new ContosoCamera
        {
            FirmwareVersion = "1.0.6.2",
            Title = "Contoso GO 2",
            FirmwareUpdateAvailable = true
        };

        private async void Application_Startup(object sender, System.Windows.StartupEventArgs e)
        {
       
        }

        public ObservableCollection<string> LoadPictures()
        {
            return new ObservableCollection<string>() {
                "pack://application:,,,/Assets/Picture1.png",
                "pack://application:,,,/Assets/Picture2.png",
                "pack://application:,,,/Assets/Picture3.png",
                "pack://application:,,,/Assets/Picture4.png",
                "pack://application:,,,/Assets/Picture5.png",
                "pack://application:,,,/Assets/Picture6.png",
                "pack://application:,,,/Assets/Picture7.png",
                "pack://application:,,,/Assets/Picture8.png",
                "pack://application:,,,/Assets/StreetSign.png",
                "pack://application:,,,/Assets/Picture9.png",
                "pack://application:,,,/Assets/Picture10.png",
                "pack://application:,,,/Assets/Picture11.png",
                "pack://application:,,,/Assets/Picture12.png",
                "pack://application:,,,/Assets/Picture13.png",
                "pack://application:,,,/Assets/Picture14.png",
                "pack://application:,,,/Assets/Picture15.png",
                "pack://application:,,,/Assets/Picture16.png",
                "pack://application:,,,/Assets/Picture17.png",
                "pack://application:,,,/Assets/Picture18.png",
                "pack://application:,,,/Assets/Picture19.png",
                "pack://application:,,,/Assets/Picture20.png",
                "pack://application:,,,/Assets/Picture21.png",
                "pack://application:,,,/Assets/Picture22.png",
                "pack://application:,,,/Assets/Picture23.png",
                "pack://application:,,,/Assets/Picture24.png",
                "pack://application:,,,/Assets/Picture25.png",
                "pack://application:,,,/Assets/Picture26.png",
                "pack://application:,,,/Assets/Picture27.png",
                "pack://application:,,,/Assets/Picture28.png",
                "pack://application:,,,/Assets/Picture29.png",
                "pack://application:,,,/Assets/Picture30.png",
                "pack://application:,,,/Assets/Picture31.png",
                "pack://application:,,,/Assets/Picture32.png",
                "pack://application:,,,/Assets/Picture33.png",
                "pack://application:,,,/Assets/Picture34.png",
                "pack://application:,,,/Assets/Picture35.png",
                "pack://application:,,,/Assets/Picture36.png",
                "pack://application:,,,/Assets/Picture37.png",
                "pack://application:,,,/Assets/Picture38.png",
                "pack://application:,,,/Assets/Picture39.png",
                "pack://application:,,,/Assets/Picture40.png",
                "pack://application:,,,/Assets/Picture41.png",
                "pack://application:,,,/Assets/Picture42.png",
                "pack://application:,,,/Assets/Picture43.png",
                "pack://application:,,,/Assets/Picture44.png",
            };
        }
    }
}