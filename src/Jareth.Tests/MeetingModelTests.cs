using Jareth.Core.Models;
using System.Text.Json;

namespace Jareth.Tests;

public class MeetingModelTests
{
    [Fact]
    public void Meeting_DefaultValues()
    {
        var meeting = new Meeting();
        Assert.NotEmpty(meeting.Id);
        Assert.Equal(string.Empty, meeting.Title);
        Assert.Equal("mic", meeting.AudioSource);
        Assert.Equal("Uncategorised", meeting.FolderCategory);
        Assert.Equal("default", meeting.SummaryStyle);
    }

    [Fact]
    public void Meeting_FormattedDuration()
    {
        var meeting = new Meeting { DurationSeconds = 3661 }; // 1 hour, 1 minute, 1 second
        Assert.Equal("01:01:01", meeting.FormattedDuration);
    }

    [Fact]
    public void Meeting_SerializesToJson()
    {
        var meeting = new Meeting
        {
            Title = "Test",
            DurationSeconds = 60,
            AudioSource = "both",
            Speakers = new List<string> { "Alice", "Bob" }
        };

        var json = JsonSerializer.Serialize(meeting);
        Assert.Contains("\"title\":\"Test\"", json);
        Assert.Contains("\"audio_source\":\"both\"", json);
        Assert.Contains("\"speakers\":[\"Alice\",\"Bob\"]", json);
    }

    [Fact]
    public void Meeting_DeserializesFromJson()
    {
        var json = """
        {
            "id": "test-id",
            "title": "Team Sync",
            "date": "2026-03-17T14:00:00",
            "duration_seconds": 3420,
            "audio_source": "both",
            "speakers": ["Speaker 1", "Speaker 2"],
            "summary_style": "default",
            "folder_category": "Work",
            "tags": [],
            "transcription_provider": "whisper-local",
            "summary_provider": "openai"
        }
        """;

        var meeting = JsonSerializer.Deserialize<Meeting>(json);
        Assert.NotNull(meeting);
        Assert.Equal("test-id", meeting.Id);
        Assert.Equal("Team Sync", meeting.Title);
        Assert.Equal(3420, meeting.DurationSeconds);
        Assert.Equal("both", meeting.AudioSource);
        Assert.Equal(2, meeting.Speakers.Count);
        Assert.Equal("Work", meeting.FolderCategory);
    }
}
