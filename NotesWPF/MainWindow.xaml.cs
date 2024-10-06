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
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using NotesWPF.Classify;
using System.Drawing;

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
          //  CheckFirmware();
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
            //CheckFirmware();
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
            string imagePath = string.Empty;
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
                        imagePath = path;
                        AddPicture(path);
                    }
                }

                // Once we have received the shared data from the ShareOperation, call ReportCompleted()
                shareArgs.ShareOperation.ReportCompleted();

                string message = "New image: " + await GetImageDescription(imagePath);


                var appNotification = new AppNotificationBuilder()
                      .AddText(message)
                      .AddButton(new AppNotificationButton("View")
                      .AddArgument("action", "ToastClick")
                      .SetButtonStyle(AppNotificationButtonStyle.Success)).BuildNotification();

                notificationManager.Show(appNotification);



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

        #region Classify
        private InferenceSession? _inferenceSession;
        private async Task<string> GetImageDescription(string imagePath)
        {
            var sampleLoadingCts = new CancellationTokenSource();
            var localModelDetails = new ClassifySampleNavigationParameter(sampleLoadingCts.Token);
            localModelDetails.RequestWaitForCompletion();
            await InitModel(localModelDetails.ModelPath);
            localModelDetails.NotifyCompletion();
            return await ClassifyImage(imagePath);
        }

        private Task InitModel(string modelPath)
        {
            return Task.Run(() =>
            {
                if (_inferenceSession != null)
                {
                    return;
                }

                SessionOptions sessionOptions = new SessionOptions();
                sessionOptions.RegisterOrtExtensions();

                _inferenceSession = new InferenceSession(modelPath, sessionOptions);
            });
        }

        private async Task<string> ClassifyImage(string filePath)
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", System.IO.Path.GetFileName(new Uri(filePath, UriKind.Absolute).LocalPath));
            var predictions = await Task.Run(() =>
            {
                Bitmap image = new Bitmap(path);

                // Resize image
                int width = 224;
                int height = 224;
                var resizedImage = BitmapFunctions.ResizeBitmap(image, width, height);
                image.Dispose();
                image = resizedImage;

                // Preprocess image
                Tensor<float> input = new DenseTensor<float>(new[] { 1, 3, 224, 224 });
                input = BitmapFunctions.PreprocessBitmapWithStdDev(image, input);
                image.Dispose();

                // Setup inputs
                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input", input)
                };

                // Run inference
                using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _inferenceSession!.Run(inputs);

                // Postprocess to get softmax vector
                IEnumerable<float> output = results[0].AsEnumerable<float>();
                return ImageNet.GetSoftmax(output);
            });

            return predictions[0].Label.Split(',')[0];
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