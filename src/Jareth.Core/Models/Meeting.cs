using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Jareth.Core.Models;

public class Meeting
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public DateTime Date { get; set; } = DateTime.Now;

    [JsonPropertyName("duration_seconds")]
    public int DurationSeconds { get; set; }

    [JsonPropertyName("audio_source")]
    public string AudioSource { get; set; } = "both";

    [JsonPropertyName("speakers")]
    public List<string> Speakers { get; set; } = new();

    [JsonPropertyName("summary_style")]
    public string SummaryStyle { get; set; } = "default";

    [JsonPropertyName("folder_category")]
    public string FolderCategory { get; set; } = "Uncategorised";

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("transcription_provider")]
    public string TranscriptionProvider { get; set; } = "whisper-local";

    [JsonPropertyName("summary_provider")]
    public string SummaryProvider { get; set; } = "openai";

    [JsonIgnore]
    public string FolderPath { get; set; } = string.Empty;

    [JsonIgnore]
    public string FormattedDuration => TimeSpan.FromSeconds(DurationSeconds).ToString(@"hh\:mm\:ss");

    [JsonIgnore]
    public string FormattedDate => Date.ToString("dd MMM yyyy, HH:mm");
}
