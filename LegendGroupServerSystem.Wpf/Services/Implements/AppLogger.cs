using CommunityToolkit.Mvvm.Messaging;
using LegendGroupServerSystem.WPf.Messages;
using LegendGroupServerSystem.WPf.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace LegendGroupServerSystem.WPf.Services.Implements;

public class AppLogger<T> : IAppLogger<T>
{
    private readonly ILogger<T> _logger;
    private readonly IMessenger _messenger;
    public AppLogger(ILogger<T> logger, IMessenger messenger)
    {
        _logger = logger;
        _messenger = messenger;
    }
    public void LogError(string message, Exception? ex = null)
    {
        if (ex != null)
        {
            _logger.LogError(ex, message);
        }
        else
        {
            _logger.LogError(message);
        }
        _messenger.Send(new AppLogMessage($"[发生错误] {message}"));
    }

    public void LogInfo(string message)
    {
        _logger.LogInformation(message);
        _messenger.Send(new AppLogMessage(message));
    }

    public void LogInfoOnlyFile(string message)
    {
        _logger.LogInformation(message);
    }
}
