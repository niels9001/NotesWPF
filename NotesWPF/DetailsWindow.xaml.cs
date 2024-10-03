using Microsoft.ML.OnnxRuntimeGenAI;
using NotesWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public partial class DetailsWindow : Window
    {
        private GenAIModel? model;
        public List<string> FieldLabels { get; set; } = new List<string> { "Name", "Address", "City", "State", "Zip" };

        public DetailsWindow(string ImagePath)
        {
            InitializeComponent();
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(ImagePath);
            bmp.EndInit();
            img.Source = bmp;
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
    }
}
