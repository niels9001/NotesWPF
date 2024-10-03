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
            await Task.Delay(10);
            return new RecognizedText();
        }
    }

    internal class TextRecognizerOptions()
    {

    }

    internal class ImageBuffer
    {
        internal static ImageBuffer CreateBufferAttachedToBitmap(SoftwareBitmap image)
        {
            return new ImageBuffer();
            //TextRecognition.GetTextFromImage(null);
        }
    }
}

public class SoftwareBitmap();

    internal class RecognizedText
    {
        public List<RecognizedTextLine> Lines { get; set; } = new();
        public double ImageAngle { get; set; }
    }

    internal class RecognizedTextLine
    {
        public string Text { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

