using System.Text.Json.Serialization;

namespace Jareth.Core.Models;

public class AppSettings
{
    [JsonPropertyName("selected_microphone")]
    public string SelectedMicrophone { get; set; } = string.Empty;

    [JsonPropertyName("default_audio_source")]
    public string DefaultAudioSource { get; set; } = "mic";

    [JsonPropertyName("theme")]
    public string Theme { get; set; } = "System";

    [JsonPropertyName("font")]
    public string Font { get; set; } = "Roboto";

    [JsonPropertyName("folders")]
    public List<string> Folders { get; set; } = new() { "Uncategorised" };

    [JsonPropertyName("encrypted_api_keys")]
    public Dictionary<string, string> EncryptedApiKeys { get; set; } = new();

    [JsonPropertyName("local_model_endpoint")]
    public string LocalModelEndpoint { get; set; } = string.Empty;

    [JsonPropertyName("jareth_folder_path")]
    public string JarethFolderPath { get; set; } = string.Empty;
}
