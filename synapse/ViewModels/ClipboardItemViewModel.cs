using CommunityToolkit.Mvvm.ComponentModel;
using synapse.Models;
using synapse.Services;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace synapse.ViewModels
{
    public partial class ClipboardItemViewModel : ObservableObject
    {
        private readonly IApplicationIconService _iconService;
        private readonly ClipboardItem _item;
        private BitmapSource? _applicationIcon;
        private bool _isIconLoaded;

        public ClipboardItemViewModel(ClipboardItem item, IApplicationIconService iconService)
        {
            _item = item;
            _iconService = iconService;
        }

        // Expose all ClipboardItem properties
        public ClipboardItem Item => _item;
        public int Id => _item.Id;
        public string Content => _item.Content;
        public string ContentType => _item.ContentType;
        public System.DateTime Timestamp => _item.Timestamp;
        public string SourceApplication => _item.SourceApplication;
        public string? WindowTitle => _item.WindowTitle;
        public string? ApplicationExecutablePath => _item.ApplicationExecutablePath;
        public int? WordCount => _item.WordCount;
        public int? CharacterCount => _item.CharacterCount;
        public int? ImageWidth => _item.ImageWidth;
        public int? ImageHeight => _item.ImageHeight;
        public string? UrlDomain => _item.UrlDomain;
        
        // Computed properties from ClipboardItem
        public string DisplayContent => _item.DisplayContent;
        public string LocalTimestamp => _item.LocalTimestamp;
        public string DisplayTitle => _item.DisplayTitle;
        public string? FormattedWordCount => _item.FormattedWordCount;
        public string? FormattedCharacterCount => _item.FormattedCharacterCount;
        public string? FormattedImageDimensions => _item.FormattedImageDimensions;
        public string? FormattedUrlDomain => _item.FormattedUrlDomain;

        public BitmapSource? ApplicationIcon
        {
            get
            {
                if (!_isIconLoaded)
                {
                    _isIconLoaded = true;
                    _ = LoadIconAsync();
                }
                return _applicationIcon;
            }
        }

        private async Task LoadIconAsync()
        {
            var icon = await _iconService.GetApplicationIconAsync(ApplicationExecutablePath);
            if (icon != null)
            {
                _applicationIcon = icon;
                OnPropertyChanged(nameof(ApplicationIcon));
            }
        }
    }
}