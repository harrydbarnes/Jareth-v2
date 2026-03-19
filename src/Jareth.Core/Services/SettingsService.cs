using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Jareth.Core.Models;

namespace Jareth.Core.Services;

public class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public SettingsService(string jarethFolderPath)
    {
        _settingsFilePath = Path.Combine(jarethFolderPath, "AppSettings.json");
    }

    public async Task<AppSettings> LoadSettingsAsync()
    {
        if (!File.Exists(_settingsFilePath))
            return new AppSettings();

        var json = await File.ReadAllTextAsync(_settingsFilePath);
        return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        var directory = Path.GetDirectoryName(_settingsFilePath);
        if (directory != null)
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(settings, JsonOptions);
        await File.WriteAllTextAsync(_settingsFilePath, json);
    }

    public async Task SaveApiKeyAsync(string keyName, string value)
    {
        var settings = await LoadSettingsAsync();

        // Encrypt using DPAPI (Windows Data Protection)
        var encrypted = ProtectedData.Protect(
            Encoding.UTF8.GetBytes(value),
            null,
            DataProtectionScope.CurrentUser);

        settings.EncryptedApiKeys[keyName] = Convert.ToBase64String(encrypted);
        await SaveSettingsAsync(settings);
    }

    public async Task<string> LoadApiKeyAsync(string keyName)
    {
        var settings = await LoadSettingsAsync();

        if (!settings.EncryptedApiKeys.TryGetValue(keyName, out var encryptedBase64))
            return string.Empty;

        try
        {
            var encrypted = Convert.FromBase64String(encryptedBase64);
            var decrypted = ProtectedData.Unprotect(
                encrypted,
                null,
                DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(decrypted);
        }
        catch (CryptographicException)
        {
            // Key was encrypted on a different machine or user — return empty
            return string.Empty;
        }
    }
}
