using System;
using System.Windows.Media.Imaging;
using synapse.Models;

namespace synapse.Services
{
    public interface IClipboardService
    {
        void StartListener(IntPtr windowHandle);
        void SetText(string text);
        void SetImage(BitmapSource image);
        void SetContent(ClipboardItem item);
        void SetContentWithoutHistory(ClipboardItem item);
    }
} 