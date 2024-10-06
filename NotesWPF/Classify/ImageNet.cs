using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NotesWPF
{
    internal class ImageNet
    {
        public static Prediction[] GetSoftmax(IEnumerable<float> output)
        {
            float sum = output.Sum(x => (float)Math.Exp(x));
            IEnumerable<float> softmax = output.Select(x => (float)Math.Exp(x) / sum);

            return softmax.Select((x, i) => new Prediction { Label = ImageNetLabels.Labels[i], Confidence = float.Parse(x.ToString("F4", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture) })
                               .OrderByDescending(x => x.Confidence)
                               .Take(5)
                               .ToArray();
        }
    }
}