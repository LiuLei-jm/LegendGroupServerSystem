using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LegendGroupServerSystem.WPf.Messages;
using LegendGroupServerSystem.WPf.Services.Interfaces;

namespace LegendGroupServerSystem.WPf.ViewModels.Pages;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IConfigurationService _configService;
    private readonly ISignalRClientService _signalRService;
    private CancellationTokenSource? _cts;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveConfigCommand))]
    private string _serverUrl = string.Empty;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveConfigCommand))]
    private string _deviceName = string.Empty;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveConfigCommand))]
    private string _apiKey = string.Empty;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveConfigCommand))]
    private bool _isInputsEnabled = true;
    [ObservableProperty]
    private string _connectionButtonContent = "连接";

    private bool CanSave() => IsInputsEnabled && !string.IsNullOrEmpty(ServerUrl) && !string.IsNullOrEmpty(DeviceName) && !string.IsNullOrEmpty(ApiKey);
    public SettingsViewModel(IConfigurationService configService, ISignalRClientService signalRService)
    {
        _configService = configService;
        _signalRService = signalRService;
    }

    [RelayCommand]
    private async Task LoadInitialDataAsync()
    {
        var config = await _configService.LoadConfigAsync();
        if (config != null)
        {
            ServerUrl = config.ServerUrl;
            ApiKey = config.ApiKey;
            DeviceName = string.IsNullOrEmpty(config.DeviceName) ? "PC-" + Environment.MachineName : config.DeviceName;
            if (!string.IsNullOrEmpty(ServerUrl) && !string.IsNullOrEmpty(ApiKey))
                await ToggleConnectionAsync();
        }
    }

    [RelayCommand]
    private async Task ToggleConnectionAsync()
    {
        if (_cts != null)
        {
            WeakReferenceMessenger.Default.Send(new AppLogMessage("用户请求断开连接..."));
            _cts.Cancel();
            await _signalRService.StopAsync();
            _cts = null;
            ConnectionButtonContent = "连接";
            IsInputsEnabled = true;
        }
        else
        {
            _cts = new CancellationTokenSource();
            ConnectionButtonContent = "断开连接";
            IsInputsEnabled = false;
            _ = _signalRService.StartAsync(ServerUrl, ApiKey, DeviceName, _cts.Token);
        }
    }
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveConfigAsync()
    {
        await _configService.SaveConfigAsync(new Models.ConnectionConfig
        {
            ServerUrl = ServerUrl,
            DeviceName = DeviceName,
            ApiKey = ApiKey
        });
        WeakReferenceMessenger.Default.Send(new AppLogMessage("配置保存成功。"));
    }
}
