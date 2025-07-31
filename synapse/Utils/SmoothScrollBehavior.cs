using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace synapse.Utils
{
    public static class SmoothScrollBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(SmoothScrollBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static readonly DependencyProperty ScrollSpeedProperty =
            DependencyProperty.RegisterAttached(
                "ScrollSpeed",
                typeof(double),
                typeof(SmoothScrollBehavior),
                new PropertyMetadata(0.3)); // Default to 30% of normal speed

        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static double GetScrollSpeed(DependencyObject obj)
        {
            return (double)obj.GetValue(ScrollSpeedProperty);
        }

        public static void SetScrollSpeed(DependencyObject obj, double value)
        {
            obj.SetValue(ScrollSpeedProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListBox listBox)
            {
                if ((bool)e.NewValue)
                {
                    listBox.PreviewMouseWheel += OnPreviewMouseWheel;
                }
                else
                {
                    listBox.PreviewMouseWheel -= OnPreviewMouseWheel;
                }
            }
            else if (d is ScrollViewer scrollViewer)
            {
                if ((bool)e.NewValue)
                {
                    scrollViewer.PreviewMouseWheel += OnPreviewMouseWheelScrollViewer;
                }
                else
                {
                    scrollViewer.PreviewMouseWheel -= OnPreviewMouseWheelScrollViewer;
                }
            }
        }

        private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                var scrollViewer = FindScrollViewer(listBox);
                if (scrollViewer != null)
                {
                    e.Handled = true;

                    var scrollSpeed = GetScrollSpeed(listBox);
                    var delta = e.Delta * scrollSpeed;

                    // Calculate new vertical offset
                    var newOffset = scrollViewer.VerticalOffset - delta;
                    newOffset = Math.Max(0, Math.Min(newOffset, scrollViewer.ScrollableHeight));

                    // Direct scroll without animation for responsiveness
                    scrollViewer.ScrollToVerticalOffset(newOffset);
                }
            }
        }

        private static void OnPreviewMouseWheelScrollViewer(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                e.Handled = true;

                var scrollSpeed = GetScrollSpeed(scrollViewer);
                var delta = e.Delta * scrollSpeed;

                // Calculate new vertical offset
                var newOffset = scrollViewer.VerticalOffset - delta;
                newOffset = Math.Max(0, Math.Min(newOffset, scrollViewer.ScrollableHeight));

                // Direct scroll without animation for responsiveness
                scrollViewer.ScrollToVerticalOffset(newOffset);
            }
        }

        private static ScrollViewer? FindScrollViewer(DependencyObject parent)
        {
            if (parent is ScrollViewer scrollViewer)
                return scrollViewer;

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                var result = FindScrollViewer(child);
                if (result != null)
                    return result;
            }

            return null;
        }
    }

    public static class ScrollViewerBehavior
    {
        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached(
                "VerticalOffset",
                typeof(double),
                typeof(ScrollViewerBehavior),
                new PropertyMetadata(0.0, OnVerticalOffsetChanged));

        public static double GetVerticalOffset(DependencyObject obj)
        {
            return (double)obj.GetValue(VerticalOffsetProperty);
        }

        public static void SetVerticalOffset(DependencyObject obj, double value)
        {
            obj.SetValue(VerticalOffsetProperty, value);
        }

        private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }
    }
}