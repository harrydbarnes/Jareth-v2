using Jareth.Core.Helpers;

namespace Jareth.Tests;

public class GreetingHelperTests
{
    [Theory]
    [InlineData(8, 0)]   // Morning, 0 meetings
    [InlineData(8, 1)]   // Morning, 1 meeting
    [InlineData(8, 2)]   // Morning, 2 meetings
    [InlineData(8, 5)]   // Morning, 3+ meetings
    [InlineData(14, 0)]  // Afternoon, 0 meetings
    [InlineData(14, 1)]  // Afternoon, 1 meeting
    [InlineData(14, 2)]  // Afternoon, 2 meetings
    [InlineData(14, 3)]  // Afternoon, 3+ meetings
    [InlineData(19, 0)]  // Evening, 0 meetings
    [InlineData(19, 1)]  // Evening, 1 meeting
    [InlineData(19, 2)]  // Evening, 2 meetings
    [InlineData(19, 4)]  // Evening, 3+ meetings
    [InlineData(23, 0)]  // Late night, 0 meetings
    [InlineData(23, 1)]  // Late night, 1 meeting
    [InlineData(23, 2)]  // Late night, 2 meetings
    [InlineData(23, 3)]  // Late night, 3+ meetings
    public void GetGreeting_ReturnsNonEmptyString(int hour, int meetingsToday)
    {
        var greeting = GreetingHelper.GetGreeting(meetingsToday, hour);
        Assert.False(string.IsNullOrWhiteSpace(greeting));
    }

    [Theory]
    [InlineData(8, 0)]
    [InlineData(14, 1)]
    [InlineData(19, 2)]
    [InlineData(23, 3)]
    public void GetGreeting_MaxSevenWords(int hour, int meetingsToday)
    {
        // Test multiple times to account for randomness
        for (int i = 0; i < 20; i++)
        {
            var greeting = GreetingHelper.GetGreeting(meetingsToday, hour);
            var wordCount = greeting.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            Assert.True(wordCount <= 7, $"Greeting '{greeting}' has {wordCount} words, expected <= 7");
        }
    }

    [Fact]
    public void GetGreeting_DefaultOverload_ReturnsString()
    {
        var greeting = GreetingHelper.GetGreeting(0);
        Assert.False(string.IsNullOrWhiteSpace(greeting));
    }
}
