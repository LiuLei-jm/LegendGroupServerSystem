using LegendGroupServerSystem.WPf.Helpers;
using LegendGroupServerSystem.WPf.Models;
using LegendGroupServerSystem.WPf.Services.Interfaces;
using System.IO;
using System.Text.Json;

namespace LegendGroupServerSystem.WPf.Services.Implements;

public class ConfigurationService : IConfigurationService
{
    private const string ConfigFileName = "config.json";
    private readonly IAppLogger<ConfigurationService> _logger;

    public ConfigurationService(IAppLogger<ConfigurationService> logger)
    {
        _logger = logger;
    }

    public async Task<ConnectionConfig?> LoadConfigAsync()
    {
        string configFilePath = Path.Combine(Helper.GetExeCurrentPath(), ConfigFileName);
        if (!File.Exists(configFilePath))
        {
            _logger.LogInfo($"配置文件不存在：{configFilePath}");
            return null!;
        }
        try
        {
            string jsonString = await File.ReadAllTextAsync(configFilePath);
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                _logger.LogInfo("配置文件内容为空");
                return null;
            }
            var config = JsonSerializer.Deserialize<ConnectionConfig>(jsonString);
            _logger.LogInfo("配置加载成功.");
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError($"加载配置文件时发生错误：{ex.Message}");
            return null!;
        }
    }

    public async Task SaveConfigAsync(ConnectionConfig config)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(config, options);
            string configFilePath = Path.Combine(Helper.GetExeCurrentPath(), ConfigFileName);
            await File.WriteAllTextAsync(configFilePath, jsonString);
            _logger.LogInfo($"配置已成功保存到 {configFilePath} 文件.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"保存配置文件时发生错误：{ex.Message}");
        }
    }

}
