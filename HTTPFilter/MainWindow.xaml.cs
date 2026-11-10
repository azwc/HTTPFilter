using System.Windows;
using System.Windows.Input;
using HTTPFilter.ViewModel;

namespace HTTPFilter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;
        
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new(this);
            DataContext = _viewModel;
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}