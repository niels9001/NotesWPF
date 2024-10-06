using NotesWPF.Models;
using System;
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

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                System.Diagnostics.Debug.WriteLine($"File Dropped: {string.Join(", ", files)}");
                // You can now process the file paths as needed
            }
        }
    }
}
