using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Metadata;
using Windows.Security.Cryptography.Core;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using Windows.UI.Popups;
using System.Text.RegularExpressions;

namespace WindowsNotificationReader
{
    internal class NotificationListener
    {
        private UserNotificationListener listener;
        private IList<uint> PreviousNotificationsList;
        public Queue<string> NewNotificationsQueue;

        // Setup all Notification Listener Class
        // Request all Access Required for the listener to work
        public void Init()
        {

            Check_Availability();
            listener = UserNotificationListener.Current;
            Request_Access();
            BackgroundTaskAccessRequest();
        }

        private async void Check_Availability()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.Notifications.Management.UserNotificationListener"))
            {
                // Listener supported!
                string SupportedMessage = "Notification Listener Available.";
                Debug.Print(SupportedMessage);
            }

            else
            {
                // Older version of Windows, no Listener
                // Close App
                string errorMessage = "Notification Listener is not supported on this version of Windows.";

                var messageDialog = new MessageDialog(errorMessage, "Error");
                await messageDialog.ShowAsync();

                Environment.Exit(0);
            }
        }

        private async void Request_Access() 
        {
           
            // And request access to the user's notifications (must be called from UI thread)
            UserNotificationListenerAccessStatus accessStatus = await listener.RequestAccessAsync();

            if (accessStatus != UserNotificationListenerAccessStatus.Allowed)
            {
                // Listener not allowed, exit the application
                Environment.Exit(0);
            }
            else
            {
                // Debug print message
                Debug.WriteLine("Notification Listener Allowed!!!");
            }

        }

        private async void BackgroundTaskAccessRequest()
        {
            // Access Request - Backgruond Task 
            BackgroundAccessStatus accessStatus =  await BackgroundExecutionManager.RequestAccessAsync(); //backgroundTask;
            
            switch (accessStatus)
            {
                case BackgroundAccessStatus.AlwaysAllowed:
                Debug.WriteLine("Background access is always allowed.");
                break;

            case BackgroundAccessStatus.AllowedSubjectToSystemPolicy:
                Debug.WriteLine("Allowed subject to system policy.");
                break;

            case BackgroundAccessStatus.DeniedBySystemPolicy:
                Debug.WriteLine("Access is denied by system policy.");
                Environment.Exit(0);
                break;

            case BackgroundAccessStatus.DeniedByUser:
                Debug.WriteLine("Access is denied by the user in battery settings.");
                Environment.Exit(0);
                break;

            case BackgroundAccessStatus.Unspecified:
                Debug.WriteLine("Background access status is unspecified.");
                Environment.Exit(0);
                break;

            default:
                Debug.WriteLine($"Unknown access status: {accessStatus}");
                Environment.Exit(0);
                break;
            }


            // Task Builder

            if (!BackgroundTaskRegistration.AllTasks.Any(i => i.Value.Name.Equals("UserNotificationChanged")))
            {
                // Specify the background task
                var builder = new BackgroundTaskBuilder()
                {
                    Name = "UserNotificationChanged"
                };

                // Set the trigger for Listener, listening to Toast Notifications
                builder.SetTrigger(new UserNotificationChangedTrigger(NotificationKinds.Toast));

                // Register the task
                builder.Register();
            }
        }

        // Intialize Listening Background Task Saving to Shared Queues
        private void StartListening()
        { 
            
        }

        public async void SyncNotifications()
        {
            Debug.Print("Notifications Changed Event: \n");
            // Get all the current notifications from the platform
            IReadOnlyList<UserNotification> userNotifications = await listener.GetNotificationsAsync(NotificationKinds.Toast);

            // Copy the currently displayed into a list of notification ID's to be removed
            var notificationsAdded = new List<uint>();

            // For each notification in the platform
            foreach (UserNotification userNotification in userNotifications)
            {
                // if the notification isnt on the prev state then is new
                if !(PreviousNotificationsList.Contains(userNotification.Id))
                {
                    // We want to KEEP it displayed, so take it out of the list
                    // of notifications to remove.
                    //notificationsAdded.Add(userNotification.Id);

                    saveToNewNotificationsQueue()
                    Debug.Print("New Notification \n");
                }

                // Otherwise it's an Already register notification
                else
                {
                    // Dont do anything
                    Debug.Print("Already existing Notification \n");
                }
            }

            PreviousNotificationsList = userNotifications;
            // Now our toBeRemoved list only contains notification ID's that no longer exist in the platform.
            // So we will remove all those notifications from the wearable.
            foreach (uint id in toBeRemoved)
            {
                //RemoveNotificationFromWearable(id);
            }
        }

        private void isValidNotification()
        {

        }
        private void saveToNewNotificationsQueue()
        {
            isValidNotification();
            //Regex pattern To Find the App Name - Chrome

            //Regex pattern To Find the Extension Name - PushBullet, etc

            //Regex pattern To Find the Sender Service Name - NotiSender, pushbullet Android

            //Catching Groups
        }

    }
}
