using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MusicSwitcher.ViewModel;

namespace MusicSwitcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Animation Animation { get; set; }
        public MainWindow(MainViewModel viewModel)
        {
            var primaryMonitorArea = SystemParameters.WorkArea;
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Left = primaryMonitorArea.Right - Width - 5;
            Top = primaryMonitorArea.Bottom - Height - 5;
            // Tray.Icon = Properties.Resources.play_button;
            DataContext = viewModel;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            Animation = new Animation(this);
            //Hide();
        }
    }
}