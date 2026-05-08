namespace LegendGroupServerSystem.WPf.Models;

public class FileDeleteCommand
{
    public string FilePath { get; set; } = string.Empty;
    public string ContentToRemove { get; set; } = string.Empty;
    public string LogMessage { get; set; } = string.Empty;
}
