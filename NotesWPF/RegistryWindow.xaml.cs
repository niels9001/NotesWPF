using Microsoft.Win32;
using Microsoft.Windows.AI.Imaging;
using NotesWPF.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NotesWPF
{
    public partial class RegistryWindow : Window
    {
        private GenAIModel? model;
        public List<string> FieldLabels { get; set; } = new List<string> { "Name", "Address", "City", "State", "Zip" };

        public RegistryWindow()
        {
            InitializeComponent();
            this.Unloaded += (s, e) => CleanUp();
            Load();
        }

        private async void Load()
        {
            var sampleLoadingCts = new CancellationTokenSource();

            var localModelDetails = new SmartPasteSampleNavigationParameter(sampleLoadingCts.Token);


            localModelDetails.RequestWaitForCompletion();
            model = await GenAIModel.CreateAsync(localModelDetails.ModelPath, localModelDetails.PromptTemplate, localModelDetails.CancellationToken);
            if (model != null)
            {
                this.SmartForm.Model = model;
                localModelDetails.NotifyCompletion();
            }


            if (localModelDetails.ShouldWaitForCompletion)
            {
                await localModelDetails.SampleLoadedCompletionSource.Task;
            }
        }


        private void CleanUp()
        {
            model?.Dispose();
        }
        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private async void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                // You can now process the file paths as needed

                await TextRecognizer.MakeAvailableAsync();
                var textRecognizer = await TextRecognizer.CreateAsync();

                var options = new TextRecognizerOptions();

                // create ImageBuffer from image
                var imageBuffer = ImageBuffer.CreateBufferAttachedToBitmap(files[0]);

                var recognizedText = await textRecognizer.RecognizeTextFromImageAsync(imageBuffer, options);

                string singleLine = string.Join(" ", recognizedText.Lines.Select(x => x.Text));
                singleLine = "Microsoft Corporation One Microsoft Way Redmond Washington 98052-6399";
                SmartForm.Analyze(singleLine);
            }
        }
    }
}
