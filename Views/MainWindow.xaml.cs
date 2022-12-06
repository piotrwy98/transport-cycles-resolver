using TransportCyclesResolver.ViewModels;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace TransportCyclesResolver.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(DialogCoordinator.Instance);
        }
    }
}
