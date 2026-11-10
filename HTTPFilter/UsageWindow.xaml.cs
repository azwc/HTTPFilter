using System.Windows;
using HTTPFilter.ViewModel;

namespace HTTPFilter;

public partial class UsageWindow : Window
{
    private UsageViewModel _viewModel;
    public UsageWindow(MainWindow mw)
    {
        InitializeComponent();
        _viewModel = new(this);
        DataContext = _viewModel;
        Owner = mw;
    }
}