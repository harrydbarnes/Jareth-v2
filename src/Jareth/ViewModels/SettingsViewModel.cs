using Jareth.Core.Services;

namespace Jareth.ViewModels;

public class SettingsViewModel
{
    private readonly ISettingsService _settingsService;
    private readonly IAudioService _audioService;
    private readonly IStorageService _storageService;

    public string SelectedMicrophone { get; set; } = string.Empty;
    public string DefaultAudioSource { get; set; } = "mic";
    public string Theme { get; set; } = "System";
    public string Font { get; set; } = "Roboto";
    public string LocalModelEndpoint { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string AppVersion { get; set; } = "1.0.0";

    public string OpenAiApiKey { get; set; } = string.Empty;
    public string AnthropicApiKey { get; set; } = string.Empty;
    public string GeminiApiKey { get; set; } = string.Empty;
    public string WhisperApiKey { get; set; } = string.Empty;

    public IReadOnlyList<string> AvailableMicrophones => _audioService.AvailableMicrophones;

    public SettingsViewModel(ISettingsService settingsService, IAudioService audioService, IStorageService storageService)
    {
        _settingsService = settingsService;
        _audioService = audioService;
        _storageService = storageService;

        StoragePath = _storageService.JarethFolderPath;
        _audioService.RefreshMicrophones();
    }

    public async Task LoadAsync()
    {
        var settings = await _settingsService.LoadSettingsAsync();

        SelectedMicrophone = settings.SelectedMicrophone;
        DefaultAudioSource = settings.DefaultAudioSource;
        Theme = settings.Theme;
        Font = settings.Font;
        LocalModelEndpoint = settings.LocalModelEndpoint;

        OpenAiApiKey = await _settingsService.LoadApiKeyAsync("openai");
        AnthropicApiKey = await _settingsService.LoadApiKeyAsync("anthropic");
        GeminiApiKey = await _settingsService.LoadApiKeyAsync("gemini");
        WhisperApiKey = await _settingsService.LoadApiKeyAsync("whisper");
    }

    public async Task SaveAsync(string openAiKey, string anthropicKey, string geminiKey, string whisperKey)
    {
        var settings = await _settingsService.LoadSettingsAsync();

        settings.SelectedMicrophone = SelectedMicrophone;
        settings.DefaultAudioSource = DefaultAudioSource;
        settings.Theme = Theme;
        settings.Font = Font;
        settings.LocalModelEndpoint = LocalModelEndpoint;

        await _settingsService.SaveSettingsAsync(settings);

        // Save API keys encrypted
        if (!string.IsNullOrEmpty(openAiKey))
            await _settingsService.SaveApiKeyAsync("openai", openAiKey);
        if (!string.IsNullOrEmpty(anthropicKey))
            await _settingsService.SaveApiKeyAsync("anthropic", anthropicKey);
        if (!string.IsNullOrEmpty(geminiKey))
            await _settingsService.SaveApiKeyAsync("gemini", geminiKey);
        if (!string.IsNullOrEmpty(whisperKey))
            await _settingsService.SaveApiKeyAsync("whisper", whisperKey);
    }
}
