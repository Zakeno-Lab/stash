using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace synapse.Utils
{
    /// <summary>
    /// Behavior that focuses a ListBox when a bound property changes to true
    /// </summary>
    public class FocusOnPropertyChangeBehavior : Behavior<ListBox>
    {
        public static readonly DependencyProperty ShouldFocusProperty =
            DependencyProperty.Register(
                nameof(ShouldFocus),
                typeof(bool),
                typeof(FocusOnPropertyChangeBehavior),
                new PropertyMetadata(false, OnShouldFocusChanged));

        public bool ShouldFocus
        {
            get => (bool)GetValue(ShouldFocusProperty);
            set => SetValue(ShouldFocusProperty, value);
        }

        private static void OnShouldFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FocusOnPropertyChangeBehavior behavior && e.NewValue is bool shouldFocus && shouldFocus)
            {
                behavior.FocusListBox();
            }
        }

        private void FocusListBox()
        {
            if (AssociatedObject == null)
                return;

            // Use dispatcher to ensure UI is ready
            AssociatedObject.Dispatcher.BeginInvoke(() =>
            {
                // First focus the ListBox
                AssociatedObject.Focus();
                
                // Then focus the selected item if any
                if (AssociatedObject.SelectedItem != null)
                {
                    var container = AssociatedObject.ItemContainerGenerator.ContainerFromItem(AssociatedObject.SelectedItem) as ListBoxItem;
                    if (container != null)
                    {
                        container.Focus();
                        System.Windows.Input.Keyboard.Focus(container);
                    }
                }
            }, DispatcherPriority.Loaded);
        }
    }
}