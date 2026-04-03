using Microsoft.UI.Xaml.Controls;
using Jareth.Core.Helpers;

namespace Jareth.Views;

public sealed partial class WelcomePage : UserControl
{
    public WelcomePage()
    {
        this.InitializeComponent();
        UpdateGreeting();
    }

    public void UpdateGreeting(int meetingsToday = 0)
    {
        GreetingText.Text = GreetingHelper.GetGreeting(meetingsToday);
    }
}
