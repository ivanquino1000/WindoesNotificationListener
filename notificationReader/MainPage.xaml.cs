using Listener;
using System.Net;
using Windows.UI.Xaml.Controls;


namespace notificationReader;

/// <summary>
/// An empty page that can be used on its own or navigated to within a <see cref="Frame">.
/// </summary>
public sealed partial class MainPage : Page
{
    public MainPage()
    {
        //NotificationListener listener = new NotificationListener();
        //listener.requestNotificationAccess();
        //listener.SubscribeBackgroundTask();
        //listener.readNotifications();

        InitializeComponent();
    }
}
