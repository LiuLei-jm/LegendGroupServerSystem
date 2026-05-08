namespace LegendGroupServerSystem.WPf.Services.Interfaces;

public interface ISignalRClientService
{
    Task StartAsync(string serverUrl, string apiKey, string deviceName, CancellationToken token);
    Task StopAsync();
}
