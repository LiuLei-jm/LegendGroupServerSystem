using CommunityToolkit.Mvvm.ComponentModel;
using LegendGroupServerSystem.WPf.ViewModels.Pages;
using LegendGroupServerSystem.WPf.Views.Pages;

namespace LegendGroupServerSystem.WPf.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly LogViewModel _logViewModel;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly object logView;
    private readonly object settingsView;

    [ObservableProperty]
    private object _currentView;

    [ObservableProperty]
    private bool _isLogSelected;

    partial void OnIsLogSelectedChanged(bool value)
    {
        if (value)
            CurrentView = logView;
    }

    [ObservableProperty]
    private bool _isSettingsSelected;

    partial void OnIsSettingsSelectedChanged(bool value)
    {
        if (value)
            CurrentView = settingsView;
    }

    public MainViewModel(LogViewModel logViewModel, SettingsViewModel settingsViewModel)
    {
        _logViewModel = logViewModel ?? throw new ArgumentNullException(nameof(logViewModel));
        _settingsViewModel =
            settingsViewModel ?? throw new ArgumentNullException(nameof(settingsViewModel));

        _settingsViewModel?.LoadInitialDataCommand.ExecuteAsync(null);

        logView = new LogView() { DataContext = _logViewModel };
        settingsView = new SettingsView() { DataContext = _settingsViewModel };

        CurrentView = logView;
    }
}
