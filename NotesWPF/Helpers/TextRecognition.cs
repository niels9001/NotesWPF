using Notes.AI.TextRecognition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace Microsoft.Windows.AI.Imaging
{
    internal class TextRecognizer
    {
        internal static async Task<TextRecognizer> CreateAsync()
        {
            await Task.Delay(10);
            return new TextRecognizer();
        }

        internal static async Task MakeAvailableAsync()
        {
            await Task.Delay(10);
        }

        internal async Task<RecognizedText> RecognizeTextFromImageAsync(object imageBuffer, TextRecognizerOptions options)
        {
            await Task.Delay(2000);
            return RecognizeText();
        }

        private RecognizedText RecognizeText()
        {
            RecognizedText recognizedText = new RecognizedText();
            recognizedText.Lines.Add(new RecognizedTextLine { Text = "ID: 4292839" });
            recognizedText.Lines.Add(new RecognizedTextLine { Text = "Version: ahbjskals" });
            recognizedText.Lines.Add(new RecognizedTextLine { Text = "FirstName: Niels" });
            recognizedText.Lines.Add(new RecognizedTextLine { Text = "LastName: Laute" });
            return recognizedText;
        }
    }

    internal class TextRecognizerOptions()
    {

    }

    internal class ImageBuffer
    {
        internal static ImageBuffer CreateBufferAttachedToBitmap(string imagePath)
        {
            return new ImageBuffer();
          
        }
    }

}
