using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace synapse.Services
{
    public class ApplicationIconService : IApplicationIconService
    {
        private readonly ConcurrentDictionary<string, BitmapSource> _memoryCache = new();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _extractionLocks = new();
        private readonly string _iconCacheDirectory;

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        public ApplicationIconService()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _iconCacheDirectory = Path.Combine(localAppData, "Synapse", "AppIcons");
            Directory.CreateDirectory(_iconCacheDirectory);
        }

        public async Task<BitmapSource?> GetApplicationIconAsync(string? executablePath)
        {
            if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
                return null;

            try
            {
                // Check memory cache first
                if (_memoryCache.TryGetValue(executablePath, out var cachedIcon))
                    return cachedIcon;

                // Get or create a lock for this specific executable path
                var lockSemaphore = _extractionLocks.GetOrAdd(executablePath, _ => new SemaphoreSlim(1, 1));
                
                await lockSemaphore.WaitAsync();
                try
                {
                    // Double-check cache after acquiring lock
                    if (_memoryCache.TryGetValue(executablePath, out cachedIcon))
                        return cachedIcon;

                    // Check file cache
                    var cacheFileName = GetCacheFileName(executablePath);
                    var cachedFilePath = Path.Combine(_iconCacheDirectory, cacheFileName);

                    if (File.Exists(cachedFilePath))
                    {
                        var iconFromFile = await LoadIconFromFileAsync(cachedFilePath);
                        if (iconFromFile != null)
                        {
                            _memoryCache.TryAdd(executablePath, iconFromFile);
                            return iconFromFile;
                        }
                    }

                    // Extract icon from executable
                    var extractedIcon = await Task.Run(() => ExtractIconFromExecutable(executablePath));
                    if (extractedIcon != null)
                    {
                        // Save to file cache
                        await SaveIconToFileAsync(extractedIcon, cachedFilePath);
                        
                        // Add to memory cache
                        _memoryCache.TryAdd(executablePath, extractedIcon);
                        
                        return extractedIcon;
                    }

                    return null;
                }
                finally
                {
                    lockSemaphore.Release();
                    
                    // Clean up lock if no longer needed
                    if (lockSemaphore.CurrentCount == 1)
                    {
                        _extractionLocks.TryRemove(executablePath, out _);
                        lockSemaphore.Dispose();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public Task ClearCacheAsync()
        {
            return Task.Run(() =>
            {
                _memoryCache.Clear();
                
                if (Directory.Exists(_iconCacheDirectory))
                {
                    foreach (var file in Directory.GetFiles(_iconCacheDirectory, "*.png"))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {
                            // Ignore deletion errors
                        }
                    }
                }
            });
        }

        private BitmapSource? ExtractIconFromExecutable(string executablePath)
        {
            IntPtr hIcon = IntPtr.Zero;
            try
            {
                hIcon = ExtractIcon(IntPtr.Zero, executablePath, 0);
                if (hIcon == IntPtr.Zero)
                    return null;

                var iconBitmap = Imaging.CreateBitmapSourceFromHIcon(
                    hIcon,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                iconBitmap.Freeze(); // Make it thread-safe
                return iconBitmap;
            }
            finally
            {
                if (hIcon != IntPtr.Zero)
                    DestroyIcon(hIcon);
            }
        }

        private async Task<BitmapSource?> LoadIconFromFileAsync(string filePath)
        {
            try
            {
                return await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                });
            }
            catch
            {
                return null;
            }
        }

        private async Task SaveIconToFileAsync(BitmapSource icon, string filePath)
        {
            try
            {
                // Generate a temporary file name to avoid conflicts
                var tempFilePath = Path.Combine(Path.GetDirectoryName(filePath)!, Path.GetRandomFileName());
                
                await Task.Run(() =>
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(icon));

                    using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        encoder.Save(fileStream);
                    }
                });

                // Try to move the temp file to the final location
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    File.Move(tempFilePath, filePath);
                }
                catch
                {
                    // If we can't move it, just clean up the temp file
                    // Another thread probably already saved the icon
                    try { File.Delete(tempFilePath); } catch { }
                }
            }
            catch
            {
                // Ignore save errors
            }
        }

        private string GetCacheFileName(string executablePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(executablePath);
            var hash = executablePath.GetHashCode().ToString("X8");
            return $"{fileName}_{hash}.png";
        }
    }
}