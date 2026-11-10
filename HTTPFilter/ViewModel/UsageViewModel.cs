using HTTPFilter.Command;

namespace HTTPFilter.ViewModel;

public class UsageViewModel : ViewModelBase
{
    private UsageWindow _uw;

    public UsageViewModel(UsageWindow uw)
    {
        _uw = uw;
        Close = new MyCommand(_Close);
    }

    public MyCommand Close { get; }

    private void _Close()
    {
        _uw.Close();
    }
}