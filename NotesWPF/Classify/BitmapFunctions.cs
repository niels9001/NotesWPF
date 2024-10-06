    using Microsoft.ML.OnnxRuntime.Tensors;
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using System.Windows.Media;

namespace NotesWPF.Classify
{
    internal class BitmapFunctions
    {
        private static readonly float[] Mean = [0.485f, 0.456f, 0.406f];
        private static readonly float[] StdDev = [0.229f, 0.224f, 0.225f];

        public static Bitmap ResizeBitmap(Bitmap originalBitmap, int newWidth, int newHeight)
        {
            Bitmap resizedBitmap = new Bitmap(newWidth, newHeight);
            using (Graphics graphics = Graphics.FromImage(resizedBitmap))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(originalBitmap, 0, 0, newWidth, newHeight);
            }

            return resizedBitmap;
        }

        public static Tensor<float> PreprocessBitmapWithStdDev(Bitmap bitmap, Tensor<float> input)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            BitmapData bmpData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            int stride = bmpData.Stride;
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(stride) * height;
            byte[] rgbValues = new byte[bytes];

            Marshal.Copy(ptr, rgbValues, 0, bytes);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * stride + x * 3;
                    byte blue = rgbValues[index];
                    byte green = rgbValues[index + 1];
                    byte red = rgbValues[index + 2];

                    input[0, 0, y, x] = ((red / 255f) - Mean[0]) / StdDev[0];
                    input[0, 1, y, x] = ((green / 255f) - Mean[1]) / StdDev[1];
                    input[0, 2, y, x] = ((blue / 255f) - Mean[2]) / StdDev[2];
                }
            }

            bitmap.UnlockBits(bmpData);

            return input;
        }
    }
}