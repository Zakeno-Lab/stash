using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace synapse.Utils
{
    /// <summary>
    /// Behavior that enables dragging a window by clicking and dragging on the attached element
    /// </summary>
    public class WindowDragBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
            }
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            }
            base.OnDetaching();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var window = Window.GetWindow(AssociatedObject);
                if (window != null)
                {
                    try
                    {
                        window.DragMove();
                    }
                    catch
                    {
                        // DragMove can throw if called at the wrong time, ignore
                    }
                }
            }
        }
    }
}