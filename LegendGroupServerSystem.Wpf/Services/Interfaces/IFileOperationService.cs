namespace LegendGroupServerSystem.WPf.Services.Interfaces;

public interface IFileOperationService
{
    Task ModifyFIleAppendAsync(string filePath, string content, string logMessage);
    Task RemoveContentFromFileAsync(string filePath, string contentToRemove, string logMessage);
}
