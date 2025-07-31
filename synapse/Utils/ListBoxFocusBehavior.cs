using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System;

namespace synapse.Utils
{
    /// <summary>
    /// Behavior that ensures the selected item in a ListBox receives keyboard focus when the window is activated
    /// </summary>
    public class ListBoxFocusBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            
            if (AssociatedObject != null)
            {
                // Hook up to the parent window's Activated event
                var window = Window.GetWindow(AssociatedObject);
                if (window != null)
                {
                    window.Activated += OnWindowActivated;
                }
                else
                {
                    // If window is not ready yet, wait for it
                    AssociatedObject.Loaded += OnListBoxLoaded;
                }
                
                // Also handle selection changes to ensure focus
                AssociatedObject.SelectionChanged += OnSelectionChanged;
            }
        }

        private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            // Only focus if user is not typing in search box
            var focusedElement = System.Windows.Input.Keyboard.FocusedElement;
            
            // If a TextBox has focus, don't steal it
            if (focusedElement is System.Windows.Controls.TextBox)
                return;
                
            // If the ListBox or one of its items already has focus, update focus to new selection
            if (focusedElement == AssociatedObject || focusedElement is ListBoxItem)
            {
                // Small delay to let the UI update
                AssociatedObject?.Dispatcher.BeginInvoke(() =>
                {
                    FocusSelectedItem();
                }, DispatcherPriority.Background);
            }
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
            {
                var window = Window.GetWindow(AssociatedObject);
                if (window != null)
                {
                    window.Activated -= OnWindowActivated;
                }
                AssociatedObject.Loaded -= OnListBoxLoaded;
                AssociatedObject.SelectionChanged -= OnSelectionChanged;
            }
            
            base.OnDetaching();
        }

        private void OnListBoxLoaded(object sender, RoutedEventArgs e)
        {
            // Now that the ListBox is loaded, we can get its parent window
            var window = Window.GetWindow(AssociatedObject);
            if (window != null)
            {
                window.Activated += OnWindowActivated;
            }
            AssociatedObject.Loaded -= OnListBoxLoaded;
        }

        private void OnWindowActivated(object sender, EventArgs e)
        {
            // Use dispatcher to ensure the UI is ready
            AssociatedObject.Dispatcher.BeginInvoke(() =>
            {
                FocusSelectedItem();
            }, DispatcherPriority.Loaded);
        }

        private void FocusSelectedItem()
        {
            if (AssociatedObject == null || AssociatedObject.SelectedItem == null)
                return;

            // First ensure the ListBox itself has focus
            if (!AssociatedObject.IsKeyboardFocusWithin)
            {
                AssociatedObject.Focus();
            }

            // Get the container for the selected item
            var container = AssociatedObject.ItemContainerGenerator.ContainerFromItem(AssociatedObject.SelectedItem) as ListBoxItem;
            
            if (container != null)
            {
                // Ensure container is visible
                container.BringIntoView();
                
                // Focus the container
                container.Focus();
                
                // Ensure keyboard focus
                System.Windows.Input.Keyboard.Focus(container);
            }
            else
            {
                // If container is not generated yet, wait for it
                AssociatedObject.ItemContainerGenerator.StatusChanged -= OnItemContainerGeneratorStatusChanged; // Remove any existing handler
                AssociatedObject.ItemContainerGenerator.StatusChanged += OnItemContainerGeneratorStatusChanged;
            }
        }

        private void OnItemContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            if (AssociatedObject.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                AssociatedObject.ItemContainerGenerator.StatusChanged -= OnItemContainerGeneratorStatusChanged;
                
                // Try again now that containers are generated
                FocusSelectedItem();
            }
        }
    }
}