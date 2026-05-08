namespace LegendGroupServerSystem.WPf.Services.Interfaces;

public interface IAppLogger<T>
{
    void LogInfo(string message);
    void LogError(string message, Exception? ex = null);
    void LogInfoOnlyFile(string message);
}
