using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jareth.Core.Helpers;
using Jareth.Core.Models;
using Jareth.Core.Services;

namespace Jareth.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly IAudioService _audio;

    [ObservableProperty] private string _theme = "System";
    [ObservableProperty] private string _font = "Roboto";
    [ObservableProperty] private string _defaultAudioSource = "Both";
    [ObservableProperty] private string _selectedMicrophone = string.Empty;
    [ObservableProperty] private string _jarethFolderPath = string.Empty;
    [ObservableProperty] private string _openAiKey = string.Empty;
    [ObservableProperty] private string _anthropicKey = string.Empty;
    [ObservableProperty] private string _geminiKey = string.Empty;
    [ObservableProperty] private string _localModelEndpoint = string.Empty;
    [ObservableProperty] private string _whisperKey = string.Empty;
    [ObservableProperty] private ObservableCollection<string> _microphoneDevices = new();
    [ObservableProperty] private string _statusMessage = string.Empty;

    public string[] ThemeOptions { get; } = { "Light", "Dark", "System" };
    public string[] FontOptions { get; } = { "Roboto", "Roboto Slab", "Roboto Serif" };
    public string[] AudioSourceOptions { get; } = { "Mic Only", "System Only", "Both" };

    public string AppVersion => System.Reflection.Assembly.GetExecutingAssembly()
        .GetName().Version?.ToString() ?? "1.0.0";

    public SettingsViewModel(ISettingsService settings, IAudioService audio)
    {
        _settings = settings;
        _audio = audio;
        LoadFromSettings();
        LoadMicrophoneDevices();
    }

    private void LoadFromSettings()
    {
        var s = _settings.Settings;
        Theme = s.Theme;
        Font = s.Font;
        DefaultAudioSource = s.DefaultAudioSource;
        SelectedMicrophone = s.DefaultMicrophoneDevice;
        JarethFolderPath = s.JarethFolderPath;
        OpenAiKey = EncryptionHelper.Decrypt(s.OpenAiKeyEncrypted);
        AnthropicKey = EncryptionHelper.Decrypt(s.AnthropicKeyEncrypted);
        GeminiKey = EncryptionHelper.Decrypt(s.GeminiKeyEncrypted);
        LocalModelEndpoint = EncryptionHelper.Decrypt(s.LocalModelEndpointEncrypted);
        WhisperKey = EncryptionHelper.Decrypt(s.WhisperKeyEncrypted);
    }

    private void LoadMicrophoneDevices()
    {
        MicrophoneDevices.Clear();
        foreach (var d in _audio.GetMicrophoneDevices())
            MicrophoneDevices.Add(d);
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        var s = _settings.Settings;
        s.Theme = Theme;
        s.Font = Font;
        s.DefaultAudioSource = DefaultAudioSource;
        s.DefaultMicrophoneDevice = SelectedMicrophone;
        s.JarethFolderPath = JarethFolderPath;
        s.OpenAiKeyEncrypted = EncryptionHelper.Encrypt(OpenAiKey);
        s.AnthropicKeyEncrypted = EncryptionHelper.Encrypt(AnthropicKey);
        s.GeminiKeyEncrypted = EncryptionHelper.Encrypt(GeminiKey);
        s.LocalModelEndpointEncrypted = EncryptionHelper.Encrypt(LocalModelEndpoint);
        s.WhisperKeyEncrypted = EncryptionHelper.Encrypt(WhisperKey);
        await _settings.SaveAsync();
        StatusMessage = "Settings saved.";
    }

    [RelayCommand]
    public void OpenJarethFolder()
    {
        if (System.IO.Directory.Exists(JarethFolderPath))
            System.Diagnostics.Process.Start("explorer.exe", JarethFolderPath);
    }
}
