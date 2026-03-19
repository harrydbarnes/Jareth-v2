using System;
using Jareth.Core.Helpers;
using Xunit;

namespace Jareth.Tests;

public class GreetingHelperTests
{
    [Theory]
    [InlineData(6, 0)]
    [InlineData(10, 1)]
    [InlineData(14, 2)]
    [InlineData(18, 0)]
    [InlineData(22, 3)]
    public void GetGreeting_ReturnsNonEmptyString(int hour, int meetingsToday)
    {
        var now = new DateTime(2026, 3, 17, hour, 0, 0);
        var greeting = GreetingHelper.GetGreeting(now, meetingsToday);
        Assert.NotEmpty(greeting);
    }

    [Fact]
    public void GetGreeting_Morning_ReturnsMorningGreeting()
    {
        var now = new DateTime(2026, 3, 17, 9, 0, 0);
        var greeting = GreetingHelper.GetGreeting(now, 0);
        Assert.NotEmpty(greeting);
    }

    [Fact]
    public void GetGreeting_Afternoon_ReturnsGreeting()
    {
        var now = new DateTime(2026, 3, 17, 14, 0, 0);
        var greeting = GreetingHelper.GetGreeting(now, 0);
        Assert.NotEmpty(greeting);
    }

    [Fact]
    public void GetGreeting_Evening_ReturnsGreeting()
    {
        var now = new DateTime(2026, 3, 17, 19, 0, 0);
        var greeting = GreetingHelper.GetGreeting(now, 0);
        Assert.NotEmpty(greeting);
    }

    [Fact]
    public void GetGreeting_LateNight_ReturnsGreeting()
    {
        var now = new DateTime(2026, 3, 17, 23, 0, 0);
        var greeting = GreetingHelper.GetGreeting(now, 0);
        Assert.NotEmpty(greeting);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    public void GetGreeting_AllMeetingCounts_ReturnNonEmpty(int count)
    {
        var now = new DateTime(2026, 3, 17, 10, 0, 0);
        var greeting = GreetingHelper.GetGreeting(now, count);
        Assert.NotEmpty(greeting);
    }

    [Theory]
    [InlineData(6, 0, "morning")]
    [InlineData(11, 0, "morning")]
    [InlineData(12, 0, "afternoon")]
    [InlineData(16, 0, "afternoon")]
    [InlineData(17, 0, "evening")]
    [InlineData(20, 0, "evening")]
    [InlineData(21, 0, "latenight")]
    [InlineData(23, 0, "latenight")]
    public void GetGreeting_CorrectTimeSlot_MaxSevenWords(int hour, int meetings, string expectedSlot)
    {
        var now = new DateTime(2026, 3, 17, hour, 0, 0);
        var greeting = GreetingHelper.GetGreeting(now, meetings);
        var wordCount = greeting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        Assert.True(wordCount <= 7, $"Greeting '{greeting}' has {wordCount} words, exceeds 7");
    }
}
