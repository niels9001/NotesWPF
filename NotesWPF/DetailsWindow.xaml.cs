using Microsoft.ML.OnnxRuntimeGenAI;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NotesWPF
{
    public partial class DetailsWindow : Window
    {
        private string imgPath;
        private Model? model;
        private MultiModalProcessor? processor;
        private TokenizerStream? tokenizerStream;
        private CancellationTokenSource? _cts;
        public DetailsWindow(string ImagePath)
        {
            InitializeComponent();
            BitmapImage bmp = new BitmapImage();
            imgPath = ImagePath;
            bmp.BeginInit();
            bmp.UriSource = new Uri(ImagePath);
            bmp.EndInit();
            img.Source = bmp;
            this.Loaded += DetailsWindow_Loaded;
            this.Unloaded += (sender, args) => Dispose();
        }

        private async void DetailsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loader.Visibility = Visibility.Visible;
            var sampleLoadingCts = new CancellationTokenSource();

            var localModelDetails = new SampleNavigationParameters(sampleLoadingCts.Token);
            localModelDetails.RequestWaitForCompletion();
            await InitModel(localModelDetails.ModelPath, localModelDetails.CancellationToken);
            localModelDetails.NotifyCompletion();
            if (!localModelDetails.CancellationToken.IsCancellationRequested)
            {
                await LoadAndDescribeImage(@"C:\Users\nielslaute\Desktop\107c1e4d-a3a0-4314-b536-6fecc8fa0d78.png");
            }
        }

        private async Task InitModel(string modelPath, CancellationToken ct)
        {
            await Task.Run(
                () =>
                {
                    try
                    {
                        // initialize the model
                        model = new Model(modelPath);
                        ct.ThrowIfCancellationRequested();

                        processor = new MultiModalProcessor(model);
                        ct.ThrowIfCancellationRequested();

                        tokenizerStream = processor.CreateStream();
                        ct.ThrowIfCancellationRequested();
                    }
                    catch
                    {
                        Dispose();
                    }
                },
                ct);
        }

        private void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();

            tokenizerStream?.Dispose();
            processor?.Dispose();
            model?.Dispose();
        }

        private async IAsyncEnumerable<string> InferStreaming(string question, string imagePath, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var images = Images.Load(imagePath);

            var prompt = $@"<|user|><|image_1|>${question}<|end|><|assistant|> ";
            string[] stopTokens = { "</s>", "<|user|>", "<|end|>", "<|assistant|>" };

            var inputTensors = processor!.ProcessImages(prompt, images);

            using GeneratorParams generatorParams = new GeneratorParams(model);
            generatorParams.SetSearchOption("max_length", 2500);
            generatorParams.SetInputs(inputTensors);

            using var generator = new Generator(model, generatorParams);
            while (!generator.IsDone())
            {
                ct.ThrowIfCancellationRequested();

                await Task.Delay(0, ct).ConfigureAwait(false);

                generator.ComputeLogits();
                ct.ThrowIfCancellationRequested();

                generator.GenerateNextToken();
                ct.ThrowIfCancellationRequested();

                var part = tokenizerStream!.Decode(generator.GetSequence(0)[^1]);

                if (stopTokens.Contains(part))
                {
                    break;
                }

                ct.ThrowIfCancellationRequested();
                yield return part;
            }
        }

        private async Task LoadAndDescribeImage(string imagePath)
        {
            Loader.Visibility = Visibility.Visible;

            _cts = new CancellationTokenSource();

            // run off the UI thread
            await Task.Run(async () =>
            {
                try
                {
                    await foreach (var part in InferStreaming("What is this image", imagePath, _cts.Token))
                    {
                        // run on the UI thread
                        if (Dispatcher.Thread == Thread.CurrentThread)
                        {
                            OutputTxt.Text += part;
                        }
                        else
                        {
                            Dispatcher.Invoke(() =>
                            {
                                OutputTxt.Text += part;
                            });
                        }
                    }
                }
                catch
                {
                }
            });

            ResetState();
        }

        private void ResetState()
        {
            _cts = null;
            Loader.Visibility = Visibility.Collapsed;
        }
    }
}