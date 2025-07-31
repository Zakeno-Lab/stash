using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace synapse.Converters
{
    /// <summary>
    /// Converter that shows/hides elements based on content type matching
    /// </summary>
    public class ContentTypeVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var contentType = value as string;
            var expectedType = parameter as string;
            
            if (string.IsNullOrEmpty(contentType) || string.IsNullOrEmpty(expectedType))
                return Visibility.Collapsed;
                
            return string.Equals(contentType, expectedType, StringComparison.OrdinalIgnoreCase) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}