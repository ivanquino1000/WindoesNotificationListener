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
using System.Xml.Linq;
using System.ServiceModel.Channels;
using System.Runtime.CompilerServices;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml.Controls;
using Windows.Media.Transcoding;
using System.Collections.ObjectModel;

namespace WindowsNotificationReader
{
    internal class NotificationListener
    {
        private UserNotificationListener listener;
        private IList<uint> PreviousNotificationsList = new List<uint>();
        public Queue<string> ToBeReadenNotiticationsQueue = new Queue<string>();
        private ObservableCollection<Transaction> Transactions = new ObservableCollection<Transaction>();

        MediaElement mediaElement = new MediaElement();
        public class NotificationsMetadata
        {
            public string ClientStatus { get; set; }
            public string ClientName { get; set; }
            public string DepositedAmount { get; set; }
        }


        // Setup all Notification Listener Class
        // Request all Access Required for the listener to work
        public void Init()
        {

            Check_Availability();
            listener = UserNotificationListener.Current;
            Request_Access();
            BackgroundTaskAccessRequest();
            GetCurrentNotifications();

        }
        private async void GetCurrentNotifications()
        {
            IReadOnlyList<UserNotification> userNotifications = await listener.GetNotificationsAsync(NotificationKinds.Toast);


            // For each notification in the platform
            foreach (UserNotification userNotification in userNotifications)
            {
                PreviousNotificationsList.Add(userNotification.Id);
            }
        }


        private async void Check_Availability()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.Notifications.Management.UserNotificationListener"))
            {
                // Listener supported!
                string SupportedMessage = "Notification Listener Available.\n";
                Debug.Print(SupportedMessage);
            }

            else
            {
                // Older version of Windows, no Listener
                // Close App
                string errorMessage = "Notification Listener is not supported on this version of Windows.\n";

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
                Debug.WriteLine("Notification Listener Allowed!!! \n");
            }

        }

        private async void BackgroundTaskAccessRequest()
        {
            // Access Request - Backgruond Task 
            BackgroundAccessStatus accessStatus =  await BackgroundExecutionManager.RequestAccessAsync(); //backgroundTask;
            
            switch (accessStatus)
            {
                case BackgroundAccessStatus.AlwaysAllowed:
                Debug.WriteLine("Background access is always allowed.\n");
                break;

            case BackgroundAccessStatus.AllowedSubjectToSystemPolicy:
                Debug.WriteLine("Allowed subject to system policy.\n");
                break;

            case BackgroundAccessStatus.DeniedBySystemPolicy:
                Debug.WriteLine("Access is denied by system policy.\n");
                Environment.Exit(0);
                break;

            case BackgroundAccessStatus.DeniedByUser:
                Debug.WriteLine("Access is denied by the user in battery settings.\n");
                Environment.Exit(0);
                break;

            case BackgroundAccessStatus.Unspecified:
                Debug.WriteLine("Background access status is unspecified.\n");
                Environment.Exit(0);
                break;

            default:
                Debug.WriteLine($"Unknown access status: {accessStatus} \n");
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


        //  Activated on Background task Triggered - UserNotificationChanged
        //  Handle New Notifications
        public async void SyncNotifications()
        {
            Debug.Print("Notifications Changed Event: \n");
            // Get all the current notifications from the platform
            IReadOnlyList<UserNotification> userNotifications = await listener.GetNotificationsAsync(NotificationKinds.Toast);

            // Copy the currently displayed into a list of notification ID's to be removed
            var AddedNotifications = new List<uint>();

            // For each notification in the platform
            int i_PrevNotifications = 0;
            foreach (UserNotification userNotification in userNotifications)
            {
                // if the notification isnt on the prev state then is new
                if (!PreviousNotificationsList.Contains(userNotification.Id) && IsValidNotification(userNotification))
                {
                    // We want to KEEP it displayed, so take it out of the list
                    // of notifications to remove.
                    //notificationsAdded.Add(userNotification.Id);
                    Debug.Print($"UNREGISTERED NOTIFICATION [new notification]\n userNotification ID: {userNotification.Id} \n");

                    await saveToTransactionsList(userNotification);
                    await saveToNewNotificationsQueue(userNotification);

                    PreviousNotificationsList.Add(userNotification.Id);
                }


                // Otherwise it's an Already registered notification
                else
                {
                    // Dont do anything
                    i_PrevNotifications += 1;
                }
            }
            Debug.Print($"Already existing Notifications: {i_PrevNotifications} \n");
        }

        //  According to the App, Service, Mobile App Sender 
        //  Extracts and Returns relevant data 
        private NotificationsMetadata getNotificationMetadata(UserNotification notification)
        {
            NotificationsMetadata metadata = new NotificationsMetadata();

            //DEBUGGING ALTERNATIVE - Checks for Same ID being accesed to the fucntion 
            Debug.WriteLine($"\n Get Data Notif Binding Received: {notification.Notification.Visual.Bindings} \n");
            Debug.WriteLine($"\n Get Data Notif Id Received: {notification.Id} \n");
            Debug.WriteLine($"\n Notification Bindings {notification.Notification.Visual.Bindings[0].GetTextElements()[1].Text} \n");

            if (notification.Notification.Visual.Bindings != null)
            {
                IList<NotificationBinding> notificationBindings = notification.Notification.Visual.Bindings;
                NotificationBinding firstBinding = notificationBindings[0];


                if (firstBinding != null && firstBinding.GetTextElements() != null)
                {
                    IReadOnlyList<AdaptiveNotificationText> textElements = firstBinding.GetTextElements();
                    int i_TextElem = 0;
                    foreach ( AdaptiveNotificationText textElement in textElements )
                    {
                        Debug.WriteLine($"¨{i_TextElem} Text Elements {textElement.Text}");
                        i_TextElem++;
                    }

                    return metadata;
                        
                }
                else
                {
                    Debug.WriteLine($"Empty Binding Text Elem\n");

                    return metadata;
                }
                    
            }
            else
            {
                Debug.WriteLine("Null Notification Bindings Elements");
                return metadata;
            }

            //string notificationText = notification.Notification.Visual.Bindings[0].GetTextElements()[1].Text;
            //if (notification.Notification.Visual.Bindings != null &&
            //notification.Notification.Visual.Bindings.Count > 0)
            //{
            //    var firstBinding = notification.Notification.Visual.Bindings[0];
            //
            //    if (firstBinding != null && firstBinding.GetTextElements() != null &&
            //        firstBinding.GetTextElements().Count > 1)
            //    {
            //
            //        string notificationText = firstBinding.GetTextElements()[1].Text;
            //
            //        // Now you can use notificationText as needed.
            //
            //        Debug.WriteLine($"Notification Metadata Extracted: {notificationText}");
            //
            //        //(?<=! )([^!]+) te envió un pago por S\/ (\d+)
            //        Regex regex = new Regex(@"(?<=! )([^!]+) te envió un pago por S\/ (\d+)");
            //
            //        Match match = regex.Match(notificationText);
            //
            //
            //        metadata.ClientStatus = "Regular";
            //
            //        if (!match.Success)
            //        {
            //            metadata.ClientName = "err";
            //            metadata.DepositedAmount = "none";
            //
            //            return metadata;
            //        }
            //
            //        string clientName = match.Groups[1].Value.Trim();
            //        string depositedAmount = match.Groups[2].Value.Trim();
            //
            //        metadata.ClientName = clientName;
            //        metadata.DepositedAmount = depositedAmount;
            //        Debug.WriteLine($"Metadata Client:  {clientName}\n " +
            //            $"Metadata Amount:  {depositedAmount}\n");
            //
            //        return metadata;
            //
            //        // Use the debugger to inspect properties:
            //        // Place breakpoints and use the debugger to check the values of
            //        // notification.Notification.Visual.Bindings, firstBinding, etc.
            //    }
            //    else
            //    {
            //        // Handle the case where the necessary elements are missing.
            //        Debug.WriteLine("Invalid :List Bindings Missing");
            //        return metadata;
            //    }
            //}
            //else
            //{
            //    // Handle the case where notification.Notification.Visual.Bindings is null or empty.
            //    Debug.WriteLine("Invalid :Null Notification");
            //    return metadata;
            //}
            //
            //
            //
        }

        // Check For specific notification Sender App, Service, Mirror App
        private bool IsValidNotification( UserNotification notification)
        {
            if (notification == null)
            {
                return false;
            }

            string appName = notification.AppInfo.DisplayInfo.DisplayName;

            // Local App Name Validation - "Chrome" 
            if (appName != "Google Chrome")
            {
                Debug.Print($"Invalid Notification: \n Local App Sender  :  {appName} \n");
                return false;
            }
            
            Debug.Print($"Valid Notification: \n Local App Sender  :  {appName} \n");

            // ! Service validator Chrome Extension = Pushbullet 
            // ! Mirrored App Sender validator: Yape, NotiSender, Messages, BCP, Interbank  
            List<string> allowedMobileSenders = new List<string> 
            { 
                //"Pushbullet: Test notification",
                "Yape: Confirmación de Pago",
                "Noti Sender: Confirmación de Pago (Yape)",
                "Notifications: Yape: Confirmacion de Pago",
                //"Messages", 
                //"BCP", 
                //"Interbank" 
            };

            string mirroredMobileSender = notification.Notification.Visual.Bindings[0].GetTextElements()[0].Text;

            if (!allowedMobileSenders.Contains(mirroredMobileSender))
            {
                Debug.Print($"Invalid Mobile App Sender  :  {mirroredMobileSender} \n");
                return false;
            }
            Debug.Print($"valid Mobile App Sender  :  {mirroredMobileSender} \n");

            return true;
  
        }
        private async Task saveToTransactionsList(UserNotification notification)
        {
           

            // Notifications Metadata Extracter
            string ClientStatus, ClientName, DepositedAmount;

            NotificationsMetadata metadata = getNotificationMetadata(notification);
            ClientStatus = metadata.ClientStatus;
            ClientName = metadata.ClientName;
            DepositedAmount = metadata.DepositedAmount;

            // Save Transactions from Notification Metadata

            Transaction notificationTransaction = new Transaction();
            notificationTransaction.ClientName = ClientName;
            notificationTransaction.Amount = decimal.Parse(DepositedAmount);

            Transactions.Add(notificationTransaction);
        }
        
        private async Task saveToNewNotificationsQueue(UserNotification notification)
        {
            

            // Notifications Metadata Extracter
            string ClientStatus, ClientName, DepositedAmount;

            NotificationsMetadata metadata = getNotificationMetadata(notification);
            ClientStatus = metadata.ClientStatus;
            ClientName = metadata.ClientName;
            DepositedAmount = metadata.DepositedAmount;

            // Notification Formated Message Builder
            string NotificationMetadataMessage = $"{ClientStatus}, {ClientName} a yapeado {DepositedAmount} soles";
            Debug.Print($"Saved To Be read Message: {NotificationMetadataMessage}\n");

            // Formated Notification Message To Post Proccesing Queue
            ToBeReadenNotiticationsQueue.Enqueue(NotificationMetadataMessage);

            //using (SpeechSynthesizer synthesizer = new SpeechSynthesizer())
            //{
            //    SpeechSynthesisStream synthesisStream = await synthesizer.SynthesizeTextToStreamAsync(NotificationMetadataMessage);
            //
            //    // Assuming MediaElement is part of your UI hierarchy
            //    mediaElement.SetSource(synthesisStream, synthesisStream.ContentType);
            //    mediaElement.Play();
            //}
        }

    }
}
