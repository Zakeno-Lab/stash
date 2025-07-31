using synapse.ViewModels;
using Wpf.Ui.Controls;

namespace synapse.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ClipboardHistoryWindow : FluentWindow
    {
        public ClipboardHistoryViewModel ViewModel { get; }

        public ClipboardHistoryWindow(ClipboardHistoryViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = ViewModel;
        }
    }
}