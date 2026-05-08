using LegendGroupServerSystem.WPf.Models;

namespace LegendGroupServerSystem.WPf.Services.Interfaces;

public interface IConfigurationService
{
    Task<ConnectionConfig?> LoadConfigAsync();
    Task SaveConfigAsync(ConnectionConfig config);
}
