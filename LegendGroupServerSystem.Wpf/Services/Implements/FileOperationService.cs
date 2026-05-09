using LegendGroupServerSystem.WPf.Helpers;
using LegendGroupServerSystem.WPf.Services.Interfaces;
using System.IO;

namespace LegendGroupServerSystem.WPf.Services.Implements;

public class FileOperationService : IFileOperationService
{
    private readonly IAppLogger<FileOperationService> _logger;

    public FileOperationService(IAppLogger<FileOperationService> logger)
    {
        _logger = logger;
    }

    public async Task ModifyFIleAppendAsync(string filePath, string content, string logMessage)
    {
        try
        {
            if (!IsValidFilePath(filePath))
            {
                _logger.LogError($"无效的文件路径： {filePath}");
                return;
            }
            string? directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                _logger.LogInfo($"创建目录：{directoryPath}");
            }
            if (!File.Exists(filePath))
            {
                await File.WriteAllTextAsync(filePath, string.Empty);
                _logger.LogInfo($"创建新文件：{filePath}");
            }
            string originalContent = await File.ReadAllTextAsync(filePath);
            if (originalContent.Contains(content))
            {
                _logger.LogInfo($"CDK已经存在，跳过写入：{content.Trim()}");
                return;
            }
            await File.AppendAllTextAsync(filePath, content);
            _logger.LogInfo(logMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError($"修改文件失败，文件：{filePath}, 错误：{ex.Message}");
        }
    }

    public async Task RemoveContentFromFileAsync(
        string filePath,
        string contentToRemove,
        string logMessage
    )
    {
        try
        {
            if (!IsValidFilePath(filePath))
            {
                _logger.LogError($"无效的文件路径：{filePath}");
                return;
            }
            if (!File.Exists(filePath))
            {
                _logger.LogError($"文件不存在，无法删除内容：{filePath}");
                return;
            }
            var originalContentLines = await File.ReadAllLinesAsync(filePath);
            if (!originalContentLines.Contains(contentToRemove))
                return;
            if (originalContentLines.Length == 0 || string.IsNullOrEmpty(contentToRemove))
                return;
            var filteredLines = originalContentLines.Where(line => !line.Contains(contentToRemove));
            await File.WriteAllLinesAsync(filePath, filteredLines);
            _logger.LogInfo(logMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError($"删除文件内容失败，文件：{filePath},错误：{ex.Message}");
        }
    }

    private bool IsValidFilePath(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;
            string fullPath = Path.GetFullPath(filePath);
            string appDirectory = Helper.GetExeCurrentPath();
            if (fullPath.StartsWith(appDirectory, StringComparison.OrdinalIgnoreCase))
                return true;
            if (
                Path.IsPathRooted(fullPath)
                && fullPath.Length >= 2
                && char.IsLetter(fullPath[0])
                && fullPath[1] == ':'
            )
                return true;
            if (filePath.Contains(".."))
                return false;
            var invalidChars = Path.GetInvalidPathChars();
            if (filePath.Any(c => invalidChars.Contains(c)))
                return false;
            return true;
        }
        catch
        {
            return false;
        }
    }
}
