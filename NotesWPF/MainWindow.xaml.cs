using Microsoft.Windows.AppNotifications.Builder;
using Microsoft.Windows.AppNotifications;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;

namespace NotesWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<string> PictureLibrary;
        public MainWindow()
        {
            InitializeComponent();
            RegisterNotifcationManager();
            PictureLibrary = (Application.Current as App)!.LoadPictures();
            PicturesListView.ItemsSource = PictureLibrary;
            CheckFirmware();
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.DefaultDirectory = "C:\\Users\\nielslaute\\Pictures";
            fileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
            fileDialog.Multiselect = true;
            fileDialog.ShowDialog();

            foreach (string path in fileDialog.FileNames)
            {
                AddPicture(path);
            }
        }


        #region Notifications
        private static AppNotificationManager notificationManager;
        private void RegisterNotifcationManager()
        {
            notificationManager = AppNotificationManager.Default;
            //notificationManager.Register();
            CheckFirmware();
        }

        private void CheckFirmware()
        {
            if ((Application.Current as App).selectedCamera.FirmwareUpdateAvailable)
            {
                var appNotification = new AppNotificationBuilder()
                      .AddText("A new firmware update is available.")
                      .AddButton(new AppNotificationButton("Install now")
                      .AddArgument("action", "ToastClick")
                      .SetButtonStyle(AppNotificationButtonStyle.Success)).BuildNotification();

                notificationManager.Show(appNotification);
            }
        }
        #endregion

        #region ShareSheet
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var activationArguments = Windows.ApplicationModel.AppInstance.GetActivatedEventArgs();
            if (activationArguments.Kind == ActivationKind.ShareTarget)
            {
                var shareArgs = activationArguments as ShareTargetActivatedEventArgs;

                if (shareArgs == null) return;

                // Get the shared data from the ShareOperation:
                shareArgs.ShareOperation.ReportStarted();
                if (shareArgs.ShareOperation.Data.Contains(StandardDataFormats.StorageItems))
                {
                    var storageItems = await shareArgs.ShareOperation.Data.GetStorageItemsAsync();
                    var sharedFilePaths = storageItems.Select(item => item.Path).ToList();

                    foreach (string path in sharedFilePaths)
                    {
                        AddPicture(path);
                    }
                }

                // Once we have received the shared data from the ShareOperation, call ReportCompleted()
                shareArgs.ShareOperation.ReportCompleted();
            }
        }
        #endregion

        #region Events
        public void AddPicture(string picturePath)
        {
            PictureLibrary.Insert(0, picturePath);
        }
        private void Config_Click(object sender, RoutedEventArgs e)
        {
            ConfigWindow configWindow = new ConfigWindow();
            configWindow.Show();
        }
        #endregion

        private void PicturesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DetailsWindow w = new DetailsWindow(e.AddedItems[0].ToString());
            w.Show();
        }

        private void Registry_Click(object sender, RoutedEventArgs e)
        {
            RegistryWindow w = new RegistryWindow();
            w.Show();
        }
    }
}