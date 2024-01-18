using System;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Notifications.Management;
using Windows.UI.Notifications;
using Windows.Foundation.Metadata;
using System.Diagnostics;
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

//GENERAL NOTES
//  ! 
//
//
//
//
//
//
//


// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0xc0a

namespace WindowsNotificationReader
{
    /// <summary>
    /// Página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //private UserNotificationListener notificationListener;
        //private NotificationListener notificationListener;
        public IServiceProvider ServiceProvider { get; private set; }
        

        public MainPage()
        {

            this.InitializeComponent();

            ConfigureServices();

            var MainNotificationListener = App.ServiceProvider.GetService<NotificationListener>();
            
            MainNotificationListener.Init();
            DataContext = MainNotificationListener;
            //Loaded += MainPage_Loaded;
        }



        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register your services or instances here
            services.AddSingleton<NotificationListener>();

            // Build the service provider
            ServiceProvider = services.BuildServiceProvider();
        }

        private void PrintSummary_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Print: \n Summary Selected ....");
        }

        private void PrintDetails_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Print: \n Complete Selected ....");
        }
        //private void MainPage_Loaded(object sender, RoutedEventArgs e)
        //{
        //    Initialize();
        //}
        //
        //private async void Initialize()
        //{
        //    InitializeNotificationListener();
        //    await CheckBackgroundTask();
        //    await OverrideBackgroundTask();
        //}
        //
        // 
        //private async Task OverrideBackgroundTask()
        //{
        //    if (!BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals("UserNotificationChanged")))
        //    {
        //        // Specify the background task
        //        var builder = new BackgroundTaskBuilder()
        //        {
        //            Name = "UserNotificationChanged"
        //        };
        //
        //        // Set the trigger for Listener, listening to Toast Notifications
        //        builder.SetTrigger(new UserNotificationChangedTrigger(NotificationKinds.Toast));
        //
        //        // Register the task
        //        builder.Register();
        //    }
        //}
        //private async Task CheckBackgroundTask()
        //
        //{
        //    BackgroundAccessStatus backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
        //
        //    switch (backgroundAccessStatus)
        //    {
        //        case BackgroundAccessStatus.Unspecified:
        //            Debug.WriteLine("Background task Access Status: Error ");
        //            break;
        //
        //        case BackgroundAccessStatus.AllowedSubjectToSystemPolicy:
        //            // Handle allowed status
        //            Debug.WriteLine("Background task Access Status: Allowed ");
        //            break;
        //
        //        case BackgroundAccessStatus.AlwaysAllowed:
        //            // Handle allowed status with always-on real-time connectivity
        //            Debug.WriteLine("Background task Access Status: Allowed");
        //            break;
        //
        //        case BackgroundAccessStatus.DeniedByUser:
        //            // Handle denied status
        //            Debug.WriteLine("Background task Access Status: Denied ");
        //            break;
        //    }
        //
        //}
        //
        //private async void InitializeNotificationListener () 
        //{
        //    if (ApiInformation.IsTypePresent("Windows.UI.Notifications.Management.UserNotificationListener"))
        //    {
        //        Debug.WriteLine("Listener supported!");
        //    }
        //
        //    else
        //    {
        //        // Windows Version incompatible !
        //        Debug.WriteLine("Listener not supported!");
        //        return;
        //    }
        //    // Get Listener 
        //    notificationListener = UserNotificationListener.Current;
        //    // Request Listener 
        //    UserNotificationListenerAccessStatus accessStatus = await notificationListener.RequestAccessAsync();
        //
        //    switch (accessStatus)
        //    {
        //        // This means the user has granted access.
        //        case UserNotificationListenerAccessStatus.Allowed:
        //
        //            // Yay! Proceed as normal
        //            System.Diagnostics.Debug.WriteLine("Listener Class Access: Confirmed");
        //            break;
        //
        //        // This means the user has denied access.
        //        // Any further calls to RequestAccessAsync will instantly
        //        // return Denied. The user must go to the Windows settings
        //        // and manually allow access.
        //        case UserNotificationListenerAccessStatus.Denied:
        //
        //            // Show UI explaining that listener features will not
        //            // work until user allows access.
        //            System.Diagnostics.Debug.WriteLine("Listener Class Access: Denied");
        //            break;
        //
        //        // This means the user closed the prompt without
        //        // selecting either allow or deny. Further calls to
        //        // RequestAccessAsync will show the dialog again.
        //        case UserNotificationListenerAccessStatus.Unspecified:
        //
        //            // Show UI that allows the user to bring up the prompt again
        //            System.Diagnostics.Debug.WriteLine("Listener Class Access: Unespecified");
        //            break;
        //    }
        //
        //    notificationListener.NotificationChanged += OnNotificationChanged; 
        //}
        //private void OnNotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args) 
        //{
        //    System.Diagnostics.Debug.WriteLine("Notification Panel Edited: Event Handler Controller");
        //
        //}
        //
    }
}
