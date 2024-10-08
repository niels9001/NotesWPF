using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using NotesWPF.Classify;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using Windows.System;

namespace NotesWPF
{
    public partial class DetailsWindow : Window
    {
        private InferenceSession? _inferenceSession;

        public DetailsWindow(string ImagePath)
        {
            InitializeComponent();
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(ImagePath);
            bmp.EndInit();
            img.Source = bmp;
            this.Unloaded += (s, e) => _inferenceSession?.Dispose();
            Load(ImagePath);
            HandleOCR(ImagePath);
        }

        public async void Load(string ImagePath)
        {
            var sampleLoadingCts = new CancellationTokenSource();
            var localModelDetails = new ClassifySampleNavigationParameter(sampleLoadingCts.Token);
            localModelDetails.RequestWaitForCompletion();
            await InitModel(localModelDetails.ModelPath);
            localModelDetails.NotifyCompletion();

            await ClassifyImage(ImagePath);

            if (localModelDetails.ShouldWaitForCompletion)
            {
                await localModelDetails.SampleLoadedCompletionSource.Task;
            }
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

        private async Task ClassifyImage(string filePath)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", Path.GetFileName(new Uri(filePath, UriKind.Absolute).LocalPath));
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

            PredictionsListView.ItemsSource = predictions;
        }

#region OCR
    private void HandleOCR(string path)
        {
            if (path.Contains("StreetSign"))
            {
                OCRPanel.Visibility = Visibility.Visible;
            }
            else
            {
                OCRPanel.Visibility = Visibility.Collapsed;
            }
        }
#endregion
    }
}
