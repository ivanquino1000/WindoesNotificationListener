
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Windows.Foundation.Metadata;
using Windows.UI.Notifications.Management;
using Windows.UI.Notifications;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace Listener
{

    public class NotificationListener
    {
        private UserNotificationListener listener;
        public IList<UserNotification> targetNotifications;
        private HashSet<string> allowedNotificationTitles;
        public NotificationListener()
        {
            listener = UserNotificationListener.Current;
            allowedNotificationTitles = new HashSet<string>
            {
                "pushbullet: test notification",
                "yape: confirmacion de pago",
            };
            requestNotificationAccess();

        }
        private Boolean isSupported()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.Notifications.Management.UserNotificationListener"))
            {
                string SupportedMessage = "Notification Listener Available.\n";
                Debug.Print(SupportedMessage);
                return true;
            }

            else
            {
                string errorMessage = "Notification Listener is not supported on this version of Windows.\n";
                Debug.Print(errorMessage);
                return false;
            }
        }
        public async Task notificationChangedHandler()
        {
            IReadOnlyList<UserNotification> userNotifications = await listener.GetNotificationsAsync(NotificationKinds.Toast);

            foreach (UserNotification userNotification in userNotifications)
            {
                if (!isValidNotification(userNotification))
                {
                    Debug.Print("Non valid notification");
                    continue;
                }

                storeNotification(userNotification);
            }
            WriteNotificationsToFile(targetNotifications, "C:\\Users\\ivan\\Desktop\\notificationListener\notifications.txt");
        }

        public static void WriteNotificationsToFile(IList<UserNotification> notifications, string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                foreach (var notification in notifications)
                {
                    // Write the notification data to the file
                    writer.WriteLine($"ID: {notification.Id}");
                    writer.WriteLine(); // Adds a blank line between notifications for readability
                }
            }

            Console.WriteLine("Notifications written to file successfully.");
        }
        private void storeNotification(UserNotification notification)
        {
            targetNotifications.Add(notification);
        }
        private async void requestNotificationAccess()
        {
            if (!isSupported())
            {
                Environment.Exit(0);
            }
            UserNotificationListenerAccessStatus accessStatus = await listener.RequestAccessAsync();

            if (accessStatus == UserNotificationListenerAccessStatus.Allowed)
            {
                Debug.WriteLine("Notification Listener Allowed!!! \n");
            }
            else
            {
                Debug.WriteLine("client Denied Notification Listener \n");
                Environment.Exit(0);
            }

        }
        public Boolean isValidNotification(UserNotification notification)
        {

            string titleText = GetNotificationText(notification, true);

            if (allowedNotificationTitles.Contains(titleText))
            {
                return true;
            }
            else
            {
                Debug.Print("Notification not allowed: ", titleText);
                return false;
            }

        }
        private string GetNotificationText(UserNotification notification, bool getTitle = true)
        {
            NotificationBinding toastBinding = notification.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);
            IReadOnlyList<AdaptiveNotificationText> textElements = toastBinding.GetTextElements();

            if (getTitle)
            {
                return textElements.FirstOrDefault()?.Text;
            }
            else
            {
                return string.Join("\n", textElements.Skip(1).Select(t => t.Text));
            }
        }
        public async void readNotifications()
        {
            listener = UserNotificationListener.Current;
            Debug.Print("Notifications Changed Event: \n");
            IReadOnlyList<UserNotification> userNotifications = await listener.GetNotificationsAsync(NotificationKinds.Toast);

            foreach (UserNotification userNotification in userNotifications)
            {
                IList<NotificationBinding> nBindings = userNotification.Notification.Visual.Bindings;

                foreach (var binding in nBindings)
                {
                    var elements = binding.GetTextElements();
                    foreach (var element in elements)
                    {
                        // Assuming the element has a 'text' property
                        var textElement = element.Text;
                        Console.WriteLine("Extracted Text: " + textElement);
                    }
                }

            }
        }


    }


}