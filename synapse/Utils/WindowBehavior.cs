using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace synapse.Utils
{
    /// <summary>
    /// Behavior for handling window events in an MVVM-compliant way
    /// </summary>
    public class WindowBehavior : Behavior<Window>
    {
        public static readonly DependencyProperty HideOnDeactivatedProperty =
            DependencyProperty.Register(nameof(HideOnDeactivated), typeof(bool), typeof(WindowBehavior), new PropertyMetadata(false));

        public static readonly DependencyProperty HideOnEscapeProperty =
            DependencyProperty.Register(nameof(HideOnEscape), typeof(bool), typeof(WindowBehavior), new PropertyMetadata(false));

        public static readonly DependencyProperty InitializeCommandProperty =
            DependencyProperty.Register(nameof(InitializeCommand), typeof(ICommand), typeof(WindowBehavior));

        public static readonly DependencyProperty HideCommandProperty =
            DependencyProperty.Register(nameof(HideCommand), typeof(ICommand), typeof(WindowBehavior));

        /// <summary>
        /// Whether to hide the window when it loses focus
        /// </summary>
        public bool HideOnDeactivated
        {
            get => (bool)GetValue(HideOnDeactivatedProperty);
            set => SetValue(HideOnDeactivatedProperty, value);
        }

        /// <summary>
        /// Whether to hide the window when Escape key is pressed
        /// </summary>
        public bool HideOnEscape
        {
            get => (bool)GetValue(HideOnEscapeProperty);
            set => SetValue(HideOnEscapeProperty, value);
        }

        /// <summary>
        /// Command to execute when window is initialized (passes window handle as parameter)
        /// </summary>
        public ICommand? InitializeCommand
        {
            get => (ICommand?)GetValue(InitializeCommandProperty);
            set => SetValue(InitializeCommandProperty, value);
        }

        /// <summary>
        /// Command to execute when window should be hidden
        /// </summary>
        public ICommand? HideCommand
        {
            get => (ICommand?)GetValue(HideCommandProperty);
            set => SetValue(HideCommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject != null)
            {
                AssociatedObject.Deactivated += OnDeactivated;
                AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
                AssociatedObject.SourceInitialized += OnSourceInitialized;
            }
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.Deactivated -= OnDeactivated;
                AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
                AssociatedObject.SourceInitialized -= OnSourceInitialized;
            }

            base.OnDetaching();
        }

        private void OnDeactivated(object? sender, EventArgs e)
        {
            if (HideOnDeactivated && HideCommand?.CanExecute(null) == true)
            {
                HideCommand.Execute(null);
            }
        }

        private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
        {
            if (HideOnEscape && e.Key == Key.Escape && HideCommand?.CanExecute(null) == true)
            {
                HideCommand.Execute(null);
            }
        }

        private void OnSourceInitialized(object? sender, EventArgs e)
        {
            if (InitializeCommand?.CanExecute(null) == true && AssociatedObject != null)
            {
                var windowHandle = new WindowInteropHelper(AssociatedObject).Handle;
                InitializeCommand.Execute(windowHandle);
            }
        }
    }
}