using synapse.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;

namespace synapse.Utils
{
    public static class IconHelper
    {
        public static readonly DependencyProperty ExecutablePathProperty =
            DependencyProperty.RegisterAttached(
                "ExecutablePath",
                typeof(string),
                typeof(IconHelper),
                new PropertyMetadata(null, OnExecutablePathChanged));

        public static string GetExecutablePath(DependencyObject obj)
        {
            return (string)obj.GetValue(ExecutablePathProperty);
        }

        public static void SetExecutablePath(DependencyObject obj, string value)
        {
            obj.SetValue(ExecutablePathProperty, value);
        }

        private static async void OnExecutablePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Image image)
                return;

            var executablePath = e.NewValue as string;
            if (string.IsNullOrWhiteSpace(executablePath))
            {
                image.Source = null;
                return;
            }

            var app = Application.Current as App;
            if (app == null)
                return;
                
            var serviceProvider = app.Services;
            var iconService = serviceProvider.GetService<IApplicationIconService>();
            if (iconService == null)
                return;

            try
            {
                var icon = await iconService.GetApplicationIconAsync(executablePath);
                image.Source = icon;
            }
            catch
            {
                // Silently fail and show no icon
                image.Source = null;
            }
        }
    }
}