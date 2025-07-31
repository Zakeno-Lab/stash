using synapse.Services;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;

namespace synapse.Converters
{
    public class ExecutablePathToIconConverter : IValueConverter
    {
        private static IApplicationIconService? _iconService;

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string executablePath || string.IsNullOrWhiteSpace(executablePath))
                return null;

            // Get the service from the DI container if not already cached
            if (_iconService == null)
            {
                var app = System.Windows.Application.Current as synapse.App;
                if (app == null)
                    return null;
                    
                var serviceProvider = app.Services;
                _iconService = serviceProvider.GetService<IApplicationIconService>();
            }

            if (_iconService == null)
                return null;

            // Run synchronously (converter must return immediately)
            var task = _iconService.GetApplicationIconAsync(executablePath);
            task.Wait(TimeSpan.FromMilliseconds(100)); // Quick timeout to avoid UI blocking
            
            return task.IsCompleted ? task.Result : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}