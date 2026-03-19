using System;
using System.Text.Json.Serialization;

namespace Jareth.Core.Models;

public class Speaker
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("meeting_id")]
    public string MeetingId { get; set; } = string.Empty;
}
