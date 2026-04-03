using Jareth.Core.Models;

namespace Jareth.Core.Services;

public interface ISettingsService
{
    Task<AppSettings> LoadSettingsAsync();
    Task SaveSettingsAsync(AppSettings settings);
    Task SaveApiKeyAsync(string keyName, string value);
    Task<string> LoadApiKeyAsync(string keyName);
}
