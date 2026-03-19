using Jareth.Core.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Jareth.Views;

public sealed partial class MeetingDetailPage : Page
{
    public Meeting? Meeting { get; private set; }

    public MeetingDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        Meeting = e.Parameter as Meeting;
        Bindings.Update();
    }
}
