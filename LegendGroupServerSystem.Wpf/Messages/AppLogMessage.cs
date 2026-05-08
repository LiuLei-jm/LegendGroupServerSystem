namespace LegendGroupServerSystem.WPf.Messages;

public class AppLogMessage
{
    public string Message { get; }
    public AppLogMessage(string message) => Message = message;
}
