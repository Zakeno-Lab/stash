using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace synapse.Utils
{
    public static class ScrollActivityBehavior
    {
        private static readonly DependencyProperty TimerProperty =
            DependencyProperty.RegisterAttached(
                "Timer",
                typeof(DispatcherTimer),
                typeof(ScrollActivityBehavior));

        private static readonly DependencyProperty LastScrollPositionProperty =
            DependencyProperty.RegisterAttached(
                "LastScrollPosition",
                typeof(double),
                typeof(ScrollActivityBehavior));

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(ScrollActivityBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static readonly DependencyProperty HideDelayProperty =
            DependencyProperty.RegisterAttached(
                "HideDelay",
                typeof(TimeSpan),
                typeof(ScrollActivityBehavior),
                new PropertyMetadata(TimeSpan.FromSeconds(2)));

        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static TimeSpan GetHideDelay(DependencyObject obj)
        {
            return (TimeSpan)obj.GetValue(HideDelayProperty);
        }

        public static void SetHideDelay(DependencyObject obj, TimeSpan value)
        {
            obj.SetValue(HideDelayProperty, value);
        }

        private static DispatcherTimer GetTimer(DependencyObject obj)
        {
            return (DispatcherTimer)obj.GetValue(TimerProperty);
        }

        private static void SetTimer(DependencyObject obj, DispatcherTimer value)
        {
            obj.SetValue(TimerProperty, value);
        }

        private static double GetLastScrollPosition(DependencyObject obj)
        {
            return (double)obj.GetValue(LastScrollPositionProperty);
        }

        private static void SetLastScrollPosition(DependencyObject obj, double value)
        {
            obj.SetValue(LastScrollPositionProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer scrollViewer = null;

            if (d is ScrollViewer sv)
            {
                scrollViewer = sv;
            }
            else if (d is ListBox listBox)
            {
                // For ListBox, wait for it to be loaded and find internal ScrollViewer
                if (listBox.IsLoaded)
                {
                    scrollViewer = FindChild<ScrollViewer>(listBox);
                }
                else
                {
                    listBox.Loaded += (s, args) =>
                    {
                        var internalScrollViewer = FindChild<ScrollViewer>(listBox);
                        if (internalScrollViewer != null)
                        {
                            if ((bool)e.NewValue)
                            {
                                AttachBehavior(internalScrollViewer);
                                // Store reference for detaching later
                                listBox.SetValue(TimerProperty, GetTimer(internalScrollViewer));
                            }
                        }
                    };
                    return;
                }
            }

            if (scrollViewer != null)
            {
                if ((bool)e.NewValue)
                {
                    AttachBehavior(scrollViewer);
                }
                else
                {
                    DetachBehavior(scrollViewer);
                }
            }
        }

        private static void AttachBehavior(ScrollViewer scrollViewer)
        {
            // Create timer for auto-hide
            var timer = new DispatcherTimer
            {
                Interval = GetHideDelay(scrollViewer)
            };
            timer.Tick += (s, e) => OnHideTimer(scrollViewer);
            SetTimer(scrollViewer, timer);

            // Subscribe to scroll events
            scrollViewer.ScrollChanged += OnScrollChanged;
            scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;

            // Initialize scroll position tracking
            SetLastScrollPosition(scrollViewer, scrollViewer.VerticalOffset);
        }

        private static void DetachBehavior(ScrollViewer scrollViewer)
        {
            // Clean up timer
            var timer = GetTimer(scrollViewer);
            if (timer != null)
            {
                timer.Stop();
                timer.Tick -= (s, e) => OnHideTimer(scrollViewer);
                SetTimer(scrollViewer, null);
            }

            // Unsubscribe from events
            scrollViewer.ScrollChanged -= OnScrollChanged;
            scrollViewer.PreviewMouseWheel -= OnPreviewMouseWheel;
        }

        private static void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                var lastPosition = GetLastScrollPosition(scrollViewer);
                var currentPosition = scrollViewer.VerticalOffset;

                // Check if actual scrolling occurred
                if (Math.Abs(currentPosition - lastPosition) > 0.1)
                {
                    SetLastScrollPosition(scrollViewer, currentPosition);
                    ShowScrollBars(scrollViewer);
                    StartHideTimer(scrollViewer);
                }
            }
        }

        private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                ShowScrollBars(scrollViewer);
                StartHideTimer(scrollViewer);
            }
        }

        private static void ShowScrollBars(ScrollViewer scrollViewer)
        {
            // Find and show scrollbar thumbs
            var verticalScrollBar = FindChild<System.Windows.Controls.Primitives.ScrollBar>(scrollViewer, "PART_VerticalScrollBar");
            var horizontalScrollBar = FindChild<System.Windows.Controls.Primitives.ScrollBar>(scrollViewer, "PART_HorizontalScrollBar");

            ShowScrollBarThumb(verticalScrollBar);
            ShowScrollBarThumb(horizontalScrollBar);
        }

        private static void ShowScrollBarThumb(System.Windows.Controls.Primitives.ScrollBar scrollBar)
        {
            if (scrollBar == null) return;

            var thumb = FindChild<System.Windows.Controls.Primitives.Thumb>(scrollBar);
            if (thumb != null)
            {
                // Stop any existing fade-out animation
                thumb.BeginAnimation(UIElement.OpacityProperty, null);
                // Show immediately
                thumb.Opacity = 1.0;
            }
        }

        private static void StartHideTimer(ScrollViewer scrollViewer)
        {
            var timer = GetTimer(scrollViewer);
            if (timer != null)
            {
                timer.Stop();
                timer.Interval = GetHideDelay(scrollViewer);
                timer.Start();
            }
        }

        private static void OnHideTimer(ScrollViewer scrollViewer)
        {
            var timer = GetTimer(scrollViewer);
            timer?.Stop();

            // Hide scrollbar thumbs
            var verticalScrollBar = FindChild<System.Windows.Controls.Primitives.ScrollBar>(scrollViewer, "PART_VerticalScrollBar");
            var horizontalScrollBar = FindChild<System.Windows.Controls.Primitives.ScrollBar>(scrollViewer, "PART_HorizontalScrollBar");

            HideScrollBarThumb(verticalScrollBar);
            HideScrollBarThumb(horizontalScrollBar);
        }

        private static void HideScrollBarThumb(System.Windows.Controls.Primitives.ScrollBar scrollBar)
        {
            if (scrollBar == null) return;

            var thumb = FindChild<System.Windows.Controls.Primitives.Thumb>(scrollBar);
            if (thumb != null)
            {
                // Stop any existing animation
                thumb.BeginAnimation(UIElement.OpacityProperty, null);
                
                // Animate to hidden
                var fadeOut = new DoubleAnimation
                {
                    To = 0.0,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };
                thumb.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            }
        }

        private static T FindChild<T>(DependencyObject parent, string childName = null) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                {
                    if (string.IsNullOrEmpty(childName) || 
                        (child is FrameworkElement fe && fe.Name == childName))
                    {
                        return typedChild;
                    }
                }

                var foundChild = FindChild<T>(child, childName);
                if (foundChild != null)
                    return foundChild;
            }

            return null;
        }
    }
}