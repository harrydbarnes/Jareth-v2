using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Jareth.Core.Models;

public class AppSettings
{
    [JsonPropertyName("theme")]
    public string Theme { get; set; } = "System";

    [JsonPropertyName("font")]
    public string Font { get; set; } = "Roboto";

    [JsonPropertyName("default_audio_source")]
    public string DefaultAudioSource { get; set; } = "Both";

    [JsonPropertyName("default_microphone_device")]
    public string DefaultMicrophoneDevice { get; set; } = string.Empty;

    [JsonPropertyName("jareth_folder_path")]
    public string JarethFolderPath { get; set; } = string.Empty;

    [JsonPropertyName("folders")]
    public List<MeetingFolder> Folders { get; set; } = new()
    {
        new MeetingFolder { Id = "default", Name = "Uncategorised", Color = "#8ACE00" },
        new MeetingFolder { Id = "all", Name = "All Meetings", Color = "#8ACE00" }
    };

    // Encrypted API keys (stored as Base64 DPAPI encrypted strings)
    [JsonPropertyName("openai_key_encrypted")]
    public string OpenAiKeyEncrypted { get; set; } = string.Empty;

    [JsonPropertyName("anthropic_key_encrypted")]
    public string AnthropicKeyEncrypted { get; set; } = string.Empty;

    [JsonPropertyName("gemini_key_encrypted")]
    public string GeminiKeyEncrypted { get; set; } = string.Empty;

    [JsonPropertyName("local_model_endpoint_encrypted")]
    public string LocalModelEndpointEncrypted { get; set; } = string.Empty;

    [JsonPropertyName("whisper_key_encrypted")]
    public string WhisperKeyEncrypted { get; set; } = string.Empty;
}
