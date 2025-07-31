using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using synapse.Models;
using synapse.Models.Events;

namespace synapse.Services
{
    public class ClipboardService : IClipboardService
    {
        private readonly IDataService _dataService;
        private readonly IEventBus _eventBus;
        private HwndSource? _hwndSource;

        // --- Fields for Deduplication ---
        private string? _lastSavedText;
        private byte[]? _lastSavedImageHash;

        // --- Fields to track programmatic clipboard changes ---
        private string? _lastProgrammaticallySetContent;
        private byte[]? _lastProgrammaticallySetImageHash;

        // --- Win32 API Definitions ---
        private const int WM_CLIPBOARDUPDATE = 0x031D;
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        public ClipboardService(IDataService dataService, IEventBus eventBus)
        {
            _dataService = dataService;
            _eventBus = eventBus;
        }

        public void StartListener(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero) throw new ArgumentNullException(nameof(windowHandle));

            _hwndSource = HwndSource.FromHwnd(windowHandle);
            _hwndSource?.AddHook(HwndHook);
            AddClipboardFormatListener(windowHandle);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_CLIPBOARDUPDATE)
            {
                OnClipboardChanged();
            }
            return IntPtr.Zero;
        }

        private async void OnClipboardChanged()
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    string clipboardText = Clipboard.GetText();
                    if (string.IsNullOrEmpty(clipboardText)) return;

                    // Check if this is content we just set programmatically
                    if (!string.IsNullOrEmpty(_lastProgrammaticallySetContent) && clipboardText == _lastProgrammaticallySetContent)
                    {
                        // Ignore programmatic clipboard changes to prevent duplicates
                        return;
                    }

                    // If we reach here, it means the content is different from what we set programmatically
                    // Now we can clear the tracking variable since we're processing new content
                    if (!string.IsNullOrEmpty(_lastProgrammaticallySetContent))
                    {
                        _lastProgrammaticallySetContent = null;
                    }

                    // Check for regular deduplication
                    if (clipboardText == _lastSavedText) return;

                    var (appName, windowTitle, executablePath) = GetForegroundWindowInfo();
                    
                    // Determine content type - check URL first, then Code, then default to Text
                    string contentType;
                    if (Utils.UrlDetector.IsUrl(clipboardText))
                    {
                        contentType = "URL";
                    }
                    else if (Utils.CodeDetector.IsCode(clipboardText))
                    {
                        contentType = "Code";
                    }
                    else
                    {
                        contentType = "Text";
                    }
                    
                    var newItem = new ClipboardItem 
                    { 
                        Content = clipboardText, 
                        ContentType = contentType, 
                        Timestamp = DateTime.UtcNow, 
                        SourceApplication = appName,
                        WindowTitle = windowTitle,
                        ApplicationExecutablePath = executablePath
                    };
                    
                    // Set type-specific metadata
                    if (contentType == "Text" || contentType == "Code")
                    {
                        var (wordCount, charCount) = Utils.ContentMetricsCalculator.CalculateTextMetrics(clipboardText);
                        newItem.WordCount = wordCount;
                        newItem.CharacterCount = charCount;
                    }
                    else if (contentType == "URL")
                    {
                        newItem.UrlDomain = Utils.UrlDetector.ExtractDomain(clipboardText);
                    }
                    
                    await _dataService.AddClipboardItemAsync(newItem);
                    _lastSavedText = clipboardText;
                    _lastSavedImageHash = null; // Reset image tracker
                    
                    // Publish event for real-time UI updates
                    _eventBus.Publish(new ClipboardItemAddedEvent(newItem));
                }
                else if (Clipboard.ContainsImage())
                {
                    var image = Clipboard.GetImage();
                    if (image == null) return;

                    var currentImageHash = ComputeImageHash(image);

                    // Check if this is an image we just set programmatically
                    if (_lastProgrammaticallySetImageHash != null && currentImageHash.SequenceEqual(_lastProgrammaticallySetImageHash))
                    {
                        // Ignore programmatic clipboard changes to prevent duplicates
                        return;
                    }

                    // If we reach here, it means the image is different from what we set programmatically
                    // Now we can clear the tracking variables since we're processing new content
                    if (!string.IsNullOrEmpty(_lastProgrammaticallySetContent))
                    {
                        _lastProgrammaticallySetContent = null;
                    }
                    if (_lastProgrammaticallySetImageHash != null)
                    {
                        _lastProgrammaticallySetImageHash = null;
                    }

                    // Check for regular deduplication
                    if (currentImageHash.SequenceEqual(_lastSavedImageHash ?? Array.Empty<byte>())) return;

                    var imagePath = HandleCopiedImage(image);
                    var (appName, windowTitle, executablePath) = GetForegroundWindowInfo();
                    var imageMetrics = Utils.ContentMetricsCalculator.CalculateImageMetrics(image);
                    
                    var newItem = new ClipboardItem 
                    { 
                        Content = imagePath, 
                        ContentType = "Image", 
                        Timestamp = DateTime.UtcNow, 
                        SourceApplication = appName,
                        WindowTitle = windowTitle,
                        ApplicationExecutablePath = executablePath,
                        ImageWidth = imageMetrics?.width,
                        ImageHeight = imageMetrics?.height
                    };
                    await _dataService.AddClipboardItemAsync(newItem);
                    _lastSavedImageHash = currentImageHash;
                    _lastSavedText = null; // Reset text tracker
                    
                    // Publish event for real-time UI updates
                    _eventBus.Publish(new ClipboardItemAddedEvent(newItem));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing clipboard content: {ex.Message}");
            }
        }

        private string HandleCopiedImage(BitmapSource image)
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var imagesFolder = Path.Combine(appDataFolder, "Synapse", "Images");
            Directory.CreateDirectory(imagesFolder);
            var fileName = $"{Guid.NewGuid()}.png";
            var filePath = Path.Combine(imagesFolder, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }
            return filePath;
        }

        private string GetSourceApplicationName()
        {
            try
            {
                IntPtr handle = GetForegroundWindow();
                GetWindowThreadProcessId(handle, out uint processId);
                var process = System.Diagnostics.Process.GetProcessById((int)processId);
                return process.MainModule?.FileVersionInfo.FileDescription ?? process.ProcessName;
            }
            catch { return "Unknown"; }
        }

        private (string appName, string? windowTitle, string? executablePath) GetForegroundWindowInfo()
        {
            try
            {
                IntPtr handle = GetForegroundWindow();
                if (handle == IntPtr.Zero)
                    return ("Unknown", null, null);

                // Get application name and executable path
                GetWindowThreadProcessId(handle, out uint processId);
                var process = System.Diagnostics.Process.GetProcessById((int)processId);
                var appName = process.MainModule?.FileVersionInfo.FileDescription ?? process.ProcessName;
                var executablePath = process.MainModule?.FileName;

                // Get window title
                const int maxTitleLength = 256;
                var titleBuilder = new System.Text.StringBuilder(maxTitleLength);
                int titleLength = GetWindowText(handle, titleBuilder, maxTitleLength);
                
                var windowTitle = titleLength > 0 ? titleBuilder.ToString() : null;
                
                // Don't return empty window titles
                if (string.IsNullOrWhiteSpace(windowTitle))
                    windowTitle = null;

                return (appName, windowTitle, executablePath);
            }
            catch 
            { 
                return ("Unknown", null, null); 
            }
        }

        private byte[] ComputeImageHash(BitmapSource image)
        {
            using (var memoryStream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(memoryStream);
                memoryStream.Position = 0;

                using (var md5 = MD5.Create())
                {
                    return md5.ComputeHash(memoryStream);
                }
            }
        }

        // New methods for setting clipboard content
        public void SetText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            
            try
            {
                Clipboard.SetText(text);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to set clipboard text: {ex.Message}");
                // Could implement retry logic or user notification here
            }
        }

        public void SetImage(BitmapSource image)
        {
            if (image == null) return;
            
            try
            {
                Clipboard.SetImage(image);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to set clipboard image: {ex.Message}");
                // Could implement retry logic or user notification here
            }
        }



        public void SetContent(ClipboardItem item)
        {
            if (item == null) return;

            try
            {
                if (item.ContentType == "Text")
                {
                    SetText(item.Content);
                }
                else if (item.ContentType == "Image")
                {
                    // Load image from file path and set to clipboard
                    var image = new BitmapImage(new Uri(item.Content));
                    SetImage(image);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to set clipboard content: {ex.Message}");
                // Could implement retry logic or user notification here
            }
        }

        public void SetContentWithoutHistory(ClipboardItem item)
        {
            if (item == null) return;

            try
            {
                if (item.ContentType == "Text" || item.ContentType == "Code" || item.ContentType == "URL")
                {
                    _lastProgrammaticallySetContent = item.Content;
                    Clipboard.SetText(item.Content);
                }
                else if (item.ContentType == "Image")
                {
                    // Load image from file path and compute hash for tracking
                    var image = new BitmapImage(new Uri(item.Content));
                    _lastProgrammaticallySetImageHash = ComputeImageHash(image);
                    Clipboard.SetImage(image);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to set clipboard content without history: {ex.Message}");
                _lastProgrammaticallySetContent = null; // Clear on error
                _lastProgrammaticallySetImageHash = null; // Clear on error
            }
        }

    }
} 