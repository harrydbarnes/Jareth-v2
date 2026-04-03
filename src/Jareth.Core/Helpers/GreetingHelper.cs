namespace Jareth.Core.Helpers;

public static class GreetingHelper
{
    private static readonly Random _random = new();

    // Greetings organized by [TimeOfDay][MeetingCount]
    // Time slots: Morning (before 12), Afternoon (12-17), Evening (17-21), LateNight (after 21)
    // Meeting counts: 0, 1, 2, 3+
    private static readonly Dictionary<string, Dictionary<int, List<string>>> Greetings = new()
    {
        ["morning"] = new()
        {
            [0] = new List<string>
            {
                "Rise and shine. Hit record.",
                "Fresh day. First meeting awaits.",
                "Morning glory. Shall we begin?",
                "Zero meetings yet. Let's fix that."
            },
            [1] = new List<string>
            {
                "One down already. Keep rolling.",
                "Back for seconds this morning.",
                "Morning's rolling. Another one?",
                "Second helping of meetings. Delicious."
            },
            [2] = new List<string>
            {
                "Hat trick incoming this morning.",
                "Two done. You're on fire.",
                "Morning marathon runner. Impressive stuff.",
                "Number three. Still before lunch."
            },
            [3] = new List<string>
            {
                "Absolute machine. Another morning meeting.",
                "Four plus? You're unstoppable today.",
                "Meeting royalty. Crown suits you.",
                "Lost count? Same. Let's go."
            }
        },
        ["afternoon"] = new()
        {
            [0] = new List<string>
            {
                "Afternoon. First one today? Easy.",
                "Post-lunch clarity. Perfect timing.",
                "Afternoon vibes. Let's capture this.",
                "Starting fresh this afternoon. Nice."
            },
            [1] = new List<string>
            {
                "Second act begins. Ready?",
                "Afternoon encore. Let's do this.",
                "One behind you. Onwards.",
                "Back again. Afternoon edition."
            },
            [2] = new List<string>
            {
                "Third time's the charm, right?",
                "Busy afternoon energy. Respect.",
                "Meeting triple threat. Impressive.",
                "Another one? You're relentless."
            },
            [3] = new List<string>
            {
                "Professional meeting attendee at this point.",
                "Legendary afternoon. Keep it going.",
                "You collect meetings like trophies.",
                "Calendar's full. You're still here."
            }
        },
        ["evening"] = new()
        {
            [0] = new List<string>
            {
                "Evening session. Bold move.",
                "First meeting at night? Mysterious.",
                "Night owl recording. Let's go.",
                "Evening start. Fashionably late."
            },
            [1] = new List<string>
            {
                "Evening follow-up. Dedicated, aren't you?",
                "Two today and still going.",
                "Back for the evening shift.",
                "Daylight's gone. Meetings haven't."
            },
            [2] = new List<string>
            {
                "Third evening meeting. Warrior mode.",
                "Sunset can't stop you. Noted.",
                "Evening hat trick. Take a bow.",
                "Still going strong this evening."
            },
            [3] = new List<string>
            {
                "Evening legend. Truly extraordinary commitment.",
                "Meetings after dark. You're committed.",
                "Night shift champion. Absolute unit.",
                "They can't schedule you enough."
            }
        },
        ["latenight"] = new()
        {
            [0] = new List<string>
            {
                "Midnight meeting? You're interesting.",
                "Late night first? Bold strategy.",
                "Burning midnight oil. Let's record.",
                "Night recording unlocked. Brave choice."
            },
            [1] = new List<string>
            {
                "Still awake? Same. Let's go.",
                "Late night sequel. Here we go.",
                "Second one past midnight? Respect.",
                "Moonlight meeting number two."
            },
            [2] = new List<string>
            {
                "Three meetings including late night?",
                "Night owl triple feature happening.",
                "You really don't sleep, huh?",
                "Third time tonight. Remarkable stamina."
            },
            [3] = new List<string>
            {
                "Sleep is optional apparently. Respect.",
                "Meeting vampire. I'm impressed.",
                "Four plus at this hour?",
                "Nocturnal meeting royalty. Bow down."
            }
        }
    };

    public static string GetGreeting(int meetingsToday)
    {
        var hour = DateTime.Now.Hour;
        var timeSlot = hour switch
        {
            < 12 => "morning",
            < 17 => "afternoon",
            < 21 => "evening",
            _ => "latenight"
        };

        var countKey = Math.Min(meetingsToday, 3);
        var greetingList = Greetings[timeSlot][countKey];
        return greetingList[_random.Next(greetingList.Count)];
    }

    public static string GetGreeting(int meetingsToday, int hour)
    {
        var timeSlot = hour switch
        {
            < 12 => "morning",
            < 17 => "afternoon",
            < 21 => "evening",
            _ => "latenight"
        };

        var countKey = Math.Min(meetingsToday, 3);
        var greetingList = Greetings[timeSlot][countKey];
        return greetingList[_random.Next(greetingList.Count)];
    }
}
