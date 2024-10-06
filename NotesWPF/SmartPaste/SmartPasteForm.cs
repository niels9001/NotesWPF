using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json.Linq;
using NotesWPF.Models;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Windows.ApplicationModel.DataTransfer;

namespace NotesWPF.Controls
{
    internal sealed class SmartPasteForm : System.Windows.Controls.Control
    {

        private GenAIModel? model;
        private List<string>? fieldLabels;
        private ProgressBar pasteProgressRing;
        public ObservableCollection<FormField> Fields { get; } = new ObservableCollection<FormField>();
        public List<string>? FieldLabels
        {
            get => (List<string>)GetValue(FieldLabelsProperty);
            set => SetValue(FieldLabelsProperty, value);
        }

        public GenAIModel Model
        {
            get => (GenAIModel)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        private string _systemPrompt = @"
You parse clumped text content to matching labels.
You will receive input in the following format:
{ ""labels"": [list of label strings], ""text"": ""arbitrary text content"" }.
You must try match subsets of the text content to the appropriate label and return it in this format: 
{ ""label1"": ""matching text"", ""label2"": ""matching text"", ""label3"": ""matching text"" }
An example input would be {""labels"": [""Name"", ""Zip"", ""Email""], ""text"": ""John Smith 94108 j.smith@gmail.com""}
And the corresponding output would be {""Name"": ""John Smith"", ""Zip"": ""94108"", ""Email"": ""j.smith@gmail.com""}
Rules: 
1. Don't make any variations on the output format. 
2. Respond with the requested output format and nothing else.
3. If you can't find a proper match for a label, exclude the label from the final results but include any other matches.
4. Always add the opening and closing braces ({ and }).
5. DO NOT PROVIDE ANY EXPLANATION OF YOUR ANSWER NO MATTER WHAT.";

        public SmartPasteForm()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SmartPasteForm), new FrameworkPropertyMetadata(typeof(SmartPasteForm)));
            pasteProgressRing = new ProgressBar();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            FieldLabels = new List<string> { "Name", "Address", "City", "State", "Zip" };
            pasteProgressRing = (ProgressBar)GetTemplateChild("PasteProgressRing");
            if (GetTemplateChild("SmartPasteButton") is Button smartPasteButton)
            {
                smartPasteButton.Click += SmartPasteButton_Click;
            }
        }

        private static void OnFieldLabelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            List<string> labels = (List<string>)e.NewValue;
            if (labels != null)
            {
                SmartPasteForm form = (SmartPasteForm)d;
                form.fieldLabels = new List<string> { "Name", "Address", "City", "State", "Zip" };
                form.Fields.Clear();
                foreach (string label in labels)
                {
                    form.Fields.Add(new FormField { Label = label, Value = string.Empty });
                }
            }
        }

        private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GenAIModel model = (GenAIModel)e.NewValue;
            if (model != null)
            {
                SmartPasteForm form = (SmartPasteForm)d;
                form.model = model;
            }
        }

        private async Task<Dictionary<string, string>> InferPasteValues(string clipboardText)
        {
            if (model == null)
            {
                return [];
            }

            string outputMessage = string.Empty;
            int maxLength = model.MaxTextLength;
            PromptInput input = new PromptInput
            {
                Labels = new List<string> { "Name", "Address", "City", "State", "Zip" },
                Text = clipboardText.Length > maxLength ?
                    clipboardText[..maxLength] :
                    clipboardText
            };

            CancellationTokenSource cts = new();
            string output = string.Empty;

            await foreach (var messagePart in model.InferStreaming(_systemPrompt, JsonSerializer.Serialize(input, SmartPasteSourceGenerationContext.Default.PromptInput), cts.Token))
            {
                outputMessage += messagePart;

                Match match = Regex.Match(outputMessage, "{([^}]*)}", RegexOptions.Multiline);
                if (match.Success)
                {
                    output = match.Value;
                    cts.Cancel();
                    break;
                }
            }

            cts.Dispose();

            if (string.IsNullOrWhiteSpace(output))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize(output, SmartPasteSourceGenerationContext.Default.DictionaryStringString) ?? [];
            }
            catch (JsonException)
            {
                return [];
            }
        }

        private async Task<string> GetTextFromClipboard()
        {
            //DataPackageView clipboardContent = Clipboard.GetContent();
            string textClipboardContent = "Microsoft Corporation One Microsoft Way Redmond Washington 98052-6399";

            //if (clipboardContent.Contains(StandardDataFormats.Text))
            //{
            //    textClipboardContent = await clipboardContent.GetTextAsync();
            //}

            return textClipboardContent;
        }

        public async void Analyze(string text)
        {
           // ((Button)sender).IsEnabled = false;
            pasteProgressRing.Visibility = Visibility.Visible;
            string clipboardText = text;
            _ = Task.Run(async () =>
            {
                Dictionary<string, string> pasteValues = await InferPasteValues(clipboardText);
                if (Dispatcher.Thread == Thread.CurrentThread)
                {
                    PasteValuesToForm(pasteValues);
                    pasteProgressRing.Visibility = Visibility.Collapsed;
                   // ((Button)sender).IsEnabled = true;
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        PasteValuesToForm(pasteValues);
                        pasteProgressRing.Visibility = Visibility.Collapsed;
                       // ((Button)sender).IsEnabled = true;
                    });
                }
            });
        }
        private async void SmartPasteButton_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void PasteValuesToForm(Dictionary<string, string> values)
        {
            foreach (FormField field in Fields)
            {
                field.Value = string.Empty;

                if (field.Label != null && values.TryGetValue(field.Label, out string? value))
                {
                    field.Value = value;
                }
            }
        }

        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
        nameof(Model),
            typeof(GenAIModel),
            typeof(SmartPasteForm),
            new PropertyMetadata(default(GenAIModel), new PropertyChangedCallback(OnModelChanged)));

        public static readonly DependencyProperty FieldLabelsProperty = DependencyProperty.Register(
            nameof(FieldLabels),
            typeof(List<string>),
            typeof(SmartPasteForm),
            new PropertyMetadata(default(List<string>), new PropertyChangedCallback(OnFieldLabelsChanged)));
    }

    internal class FormField : ObservableObject
    {
        private string? label;
        private string? value;
        public string? Label
        {
            get => label;
            set => SetProperty(ref label, value);
        }

        public string? Value
        {
            get => this.value;
            set => SetProperty(ref this.value, value);
        }
    }

    internal class PromptInput
    {
        public List<string>? Labels { get; set; }
        public string? Text { get; set; }
    }

    [JsonSourceGenerationOptions(WriteIndented = true, AllowTrailingCommas = true)]
    [JsonSerializable(typeof(PromptInput))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    internal partial class SmartPasteSourceGenerationContext : JsonSerializerContext
    {
    }
}