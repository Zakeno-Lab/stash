using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace synapse.Services
{
    public interface IApplicationIconService
    {
        /// <summary>
        /// Gets the icon for an application from its executable path
        /// </summary>
        /// <param name="executablePath">The full path to the executable</param>
        /// <returns>A BitmapSource containing the application icon, or null if extraction fails</returns>
        Task<BitmapSource?> GetApplicationIconAsync(string? executablePath);

        /// <summary>
        /// Clears the icon cache
        /// </summary>
        Task ClearCacheAsync();
    }
}