using System;
using System.Globalization;
using System.Linq;
using System.Windows.Media.Imaging;

namespace synapse.Utils
{
    public static class ContentMetricsCalculator
    {
        public static (int wordCount, int characterCount) CalculateTextMetrics(string text)
        {
            if (string.IsNullOrEmpty(text))
                return (0, 0);

            var characterCount = text.Length;
            
            var words = text.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var wordCount = words.Length;

            return (wordCount, characterCount);
        }

        public static (int width, int height)? CalculateImageMetrics(BitmapSource image)
        {
            if (image == null)
                return null;

            try
            {
                return ((int)image.PixelWidth, (int)image.PixelHeight);
            }
            catch
            {
                return null;
            }
        }

        public static string FormatTextMetrics(int wordCount, int characterCount)
        {
            if (wordCount == 0 && characterCount == 0)
                return "Empty";

            if (wordCount == 1)
                return $"1 word, {characterCount:N0} character{(characterCount == 1 ? "" : "s")}";

            return $"{wordCount:N0} words, {characterCount:N0} characters";
        }

        public static string FormatImageMetrics(int width, int height)
        {
            return $"{width:N0} × {height:N0} pixels";
        }
    }
}