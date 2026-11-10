using System.Windows;
using HTTPFilter.Command;
using Listener_Core;

namespace HTTPFilter.ViewModel;

public class MainWindowViewModel : ViewModelBase, IFunc
{
    private Core? _core;
    private MainWindow _mw;

    public MainWindowViewModel(MainWindow mw)
    {
        _mw = mw;
        _core = new Core();
        _startIsEnabled = !_core.IsRunning();
        _stopIsEnabled = _core.IsRunning();
        _isVisible = Visibility.Hidden;
        Start = new MyCommand(_Start);
        Stop = new MyCommand(_StopBtn);
        ShowUsage = new MyCommand(_ShowUsage);
        CloseWindow = new MyCommand(_Close);
    }

    private bool _startIsEnabled;

    public bool StartIsEnabled
    {
        get => !_core.IsRunning();
        set
        {
            _startIsEnabled = value;
            OnPropertyChanged();
        }
    }

    private bool _stopIsEnabled;

    public bool StopIsEnabled
    {
        get => _core.IsRunning();
        set
        {
            _stopIsEnabled = value;
            OnPropertyChanged();
        }
    }

    private Visibility _isVisible;

    public Visibility IsVisible
    {
        get => _isVisible;
        set
        {
            _isVisible = value;
            OnPropertyChanged();
        }
    }

    private string? _rUrl;

    public string RUrl
    {
        get => _rUrl??"获取失败！";
        set
        {
            _rUrl = value;
            // NotifyChanged(nameof(RUrl));
            OnPropertyChanged();
        }
    }

    public MyCommand Start { get; }
    public MyCommand Stop { get; }
    public MyCommand CloseWindow { get; }
    public MyCommand ShowUsage { get; }

    public void OnGetUrl(string url)
    {
        IsVisible = Visibility.Visible;
        RUrl = url;
        _Stop();
    }

    private void _Start()
    {
        IsVisible = Visibility.Visible;
        RUrl = "捕获中...";
        _core!.TitaniumProxy(this);
        StartIsEnabled = _core.IsRunning();
        StopIsEnabled = !_core.IsRunning();
    }

    private void _StopBtn()
    {
        _Stop();
        IsVisible = Visibility.Hidden;
        RUrl = "";
    }

    private void _Stop()
    {
        _core?.Stop();
        StartIsEnabled = !_core.IsRunning();
        StopIsEnabled = _core.IsRunning();
    }

    private void _ShowUsage()
    {
        new UsageWindow(_mw).ShowDialog();
    }
    private void _Close()
    {
        _Stop();
        _mw.Close();
    }
}