namespace LegendGroupServerSystem.WPf.Models;

public class FileWriteCommand
{
    public string FilePath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string LogMessage { get; set; } = string.Empty;
}
