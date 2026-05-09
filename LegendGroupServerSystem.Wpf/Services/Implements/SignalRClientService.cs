using LegendGroupServerSystem.WPf.Models;
using LegendGroupServerSystem.WPf.Services.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;

namespace LegendGroupServerSystem.WPf.Services.Implements;

public class SignalRClientService : ISignalRClientService, IDisposable
{
    private HubConnection? _hubConnection;
    private readonly IFileOperationService _fileService;
    private readonly IAppLogger<SignalRClientService> _logger;

    private Func<Exception?, Task>? _closeHandler;
    private Func<Exception?, Task>? _reconnectingHandler;
    private Func<string?, Task>? _reconnectedHandler;
    private List<IDisposable> _hubMethodSubscriptions = new List<IDisposable>();
    private CancellationTokenSource? _cts;
    public SignalRClientService(IFileOperationService fileService, IAppLogger<SignalRClientService> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }
    public async Task StartAsync(string serverUrl, string apiKey, string deviceName, CancellationToken token)
    {
        if (_cts != null)
        {
            _cts.Cancel();
            await StopAsync();
        }
        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        _ = ConnectInLoopAsync(serverUrl, apiKey, deviceName, token);
    }

    public async Task StopAsync()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
        await StopInternalAsync();
    }

    private async Task ConnectInLoopAsync(string serverUrl, string apiKey, string deviceName, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (_hubConnection != null)
            {
                await StopInternalAsync();
            }
            var connectionTcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var ctsRegistration = token.Register(() => connectionTcs.TrySetResult(token));

            try
            {
                string urlWithKey = $"{serverUrl}/filePushHub?apiKey={Uri.EscapeDataString(apiKey)}&deviceName={deviceName}";
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(urlWithKey)
                    .WithAutomaticReconnect()
                    .Build();
                _closeHandler = (error) =>
                {
                    if (!token.IsCancellationRequested) _logger.LogError($"与服务器连接断开：{error?.Message}");
                    connectionTcs.TrySetResult(null);
                    return Task.CompletedTask;
                };
                _hubConnection.Closed += _closeHandler;
                _reconnectingHandler = (error) =>
                {
                    _logger.LogInfo("网络波动，正在尝试自动恢复连接...");
                    return Task.CompletedTask;
                };
                _hubConnection.Reconnecting += _reconnectingHandler;
                _reconnectedHandler = (connectionId) =>
                {
                    _logger.LogInfo("网络已恢复，自动重连成功！");
                    return Task.CompletedTask;
                };
                _hubConnection.Reconnected += _reconnectedHandler;
                _hubMethodSubscriptions.Add(_hubConnection.On<FileWriteCommand>("ReceiveWriteCommand", async (cmd) =>
                               {
                                   await _fileService.ModifyFIleAppendAsync(cmd.FilePath, cmd.Content, cmd.LogMessage);
                               }));
                _hubMethodSubscriptions.Add(_hubConnection.On<FileDeleteCommand>("ReceiveDeleteCommand", async (cmd) =>
                               {
                                   await _fileService.RemoveContentFromFileAsync(cmd.FilePath, cmd.ContentToRemove, cmd.LogMessage);
                               }));
                _logger.LogInfo("正在尝试连接服务器...");
                await _hubConnection.StartAsync(token);
                _logger.LogInfo("成功连接服务器！等待指令...");
                await connectionTcs.Task;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInfo("连接过程已取消.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError($"连接失败", ex);
            }
            if (!token.IsCancellationRequested)
            {
                int retryDelay = 30000;
                _logger.LogInfo($"将在 {retryDelay / 1000} 秒后尝试重新连接...");
                try
                {
                    await Task.Delay(retryDelay, token);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInfo("已停止重试.");
                    break;
                }
            }
        }
    }

    private async Task StopInternalAsync()
    {
        if (_hubConnection != null)
        {
            if (_closeHandler != null) _hubConnection.Closed -= _closeHandler;
            if (_reconnectingHandler != null) _hubConnection.Reconnecting -= _reconnectingHandler;
            if (_reconnectedHandler != null) _hubConnection.Reconnected -= _reconnectedHandler;
            foreach (var disposable in _hubMethodSubscriptions)
            {
                disposable.Dispose();
            }
            _hubMethodSubscriptions.Clear();
            try
            {
                await _hubConnection.StopAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"停止 SignalR 连接时发生错误：{ex.Message}", ex);
            }
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
            _closeHandler = null;
            _reconnectingHandler = null;
            _reconnectedHandler = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                StopAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"在 Dispose 中停止 SignalR 服务时发生错误：{ex.Message}", ex);
            }
            if (_cts != null)
            {
                _cts.Dispose();
                _cts = null;
            }
            _hubMethodSubscriptions?.Clear();
        }
    }
}
