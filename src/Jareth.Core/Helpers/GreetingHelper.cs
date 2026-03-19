using System;
using System.Collections.Generic;

namespace Jareth.Core.Helpers;

public static class GreetingHelper
{
    private static readonly Random _rng = new();

    // [TimeOfDay][MeetingCount] => list of greetings (max 7 words each)
    private static readonly Dictionary<string, Dictionary<int, List<string>>> _greetings = new()
    {
        ["morning"] = new()
        {
            [0] = new() {
                "Morning. Let's make today legendary.",
                "Rise and record. You've got this.",
                "Fresh day, fresh meeting. Let's go.",
                "Good morning. Ready to capture everything.",
                "Morning energy locked in. Start when ready."
            },
            [1] = new() {
                "One down already. Impressive morning energy.",
                "Back again? Morning's treating you well.",
                "Meeting two. Still caffeinated, I assume.",
                "Second one already. You're on fire.",
                "Another morning meeting. You absolute machine."
            },
            [2] = new() {
                "Third morning meeting. Respect. Truly.",
                "Three meetings before noon. Legendary status.",
                "You live in meetings. Morning warrior.",
                "Morning number three. Still standing, though.",
                "Third round. Jareth salutes your dedication."
            },
            [3] = new() {
                "This many meetings by noon? Hero.",
                "Morning meetings: you've collected them all.",
                "Four-plus meetings? Morning MVP right here.",
                "Unstoppable. Jareth is proud of you.",
                "You're the morning meeting champion. Always."
            }
        },
        ["afternoon"] = new()
        {
            [0] = new() {
                "Good afternoon. Ready when you are.",
                "Afternoon. Fresh start, no judgment here.",
                "Post-lunch meeting. Bold and strategic move.",
                "Afternoon session loading. Jareth stands ready.",
                "Good afternoon. Let's make it count."
            },
            [1] = new() {
                "Second meeting. Afternoon is yours to conquer.",
                "One recorded, one to go. Let's move.",
                "Afternoon round two. You're still sharp.",
                "Back for more? Afternoon warrior energy.",
                "Second one already. Afternoon's treating you."
            },
            [2] = new() {
                "Third meeting today. You're a machine.",
                "Afternoon number three. Still going strong.",
                "Meeting three. Jareth takes notes diligently.",
                "Three meetings deep. You're genuinely unstoppable.",
                "Third time today. Dedication is showing."
            },
            [3] = new() {
                "Four meetings today. You're basically a legend.",
                "This many meetings? Absolute afternoon warrior.",
                "Jareth has lost count. You haven't.",
                "Meeting overload mode: activated. Let's record.",
                "So many meetings. Jareth respects the hustle."
            }
        },
        ["evening"] = new()
        {
            [0] = new() {
                "Evening call. Dedication noted by Jareth.",
                "Evening meeting? You're committed, aren't you.",
                "Good evening. One more for the record.",
                "Evening mode activated. Let's capture this.",
                "Late-day energy. Jareth appreciates the commitment."
            },
            [1] = new() {
                "Second today, first this evening. Impressive.",
                "Evening round two. Still going, seriously.",
                "Back again in the evening. Respect.",
                "Two meetings today. Evening hustle confirmed.",
                "Evening meeting two. You're still sharp?"
            },
            [2] = new() {
                "Three meetings and it's still evening. Wow.",
                "Third meeting. Evening warrior behaviour, honestly.",
                "Evening number three. Jareth is amazed.",
                "Three today? Evening dedication is real.",
                "Still recording at this hour? Legend confirmed."
            },
            [3] = new() {
                "Four-plus meetings today. Evening hero status.",
                "This many meetings and still evening? Respect.",
                "Jareth is tired for you. Let's go.",
                "Unstoppable today. Evening MVP without question.",
                "All these meetings. You're a force, honestly."
            }
        },
        ["latenight"] = new()
        {
            [0] = new() {
                "Late night call. Respect. Let's go.",
                "Midnight meeting? Jareth never sleeps either.",
                "Late night. Make this one count.",
                "Late night session. Jareth is right here.",
                "Night owl meeting. Jareth approves of this."
            },
            [1] = new() {
                "Two today, one at midnight. Dedication.",
                "Late night round two. You're committed.",
                "Second meeting this late? Jareth respects it.",
                "Evening and late night. You don't stop.",
                "Still recording? Late night warrior confirmed."
            },
            [2] = new() {
                "Third meeting and it's after dark. Wow.",
                "Three meetings, now this late. Extraordinary.",
                "Late night number three. Jareth salutes you.",
                "This many meetings after midnight? Hero behaviour.",
                "Three recorded, going for four. Respect."
            },
            [3] = new() {
                "Four-plus at this hour? Absolute legend.",
                "Jareth's lost count. You clearly haven't.",
                "Still going after all those meetings? Wild.",
                "Meeting collector. Late-night edition. Let's go.",
                "This is dedication. Jareth bows respectfully."
            }
        }
    };

    public static string GetGreeting(DateTime now, int meetingsToday)
    {
        var timeKey = now.Hour switch
        {
            < 12 => "morning",
            < 17 => "afternoon",
            < 21 => "evening",
            _ => "latenight"
        };

        var countKey = meetingsToday switch
        {
            0 => 0,
            1 => 1,
            2 => 2,
            _ => 3
        };

        var list = _greetings[timeKey][countKey];
        return list[_rng.Next(list.Count)];
    }
}
