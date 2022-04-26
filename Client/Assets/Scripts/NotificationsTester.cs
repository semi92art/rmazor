// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using Common;
// using NotificationSamples;
// using UnityEngine;
// #if UNITY_ANDROID
// using NotificationSamples.Android;
// using Unity.Notifications.Android;
// #elif UNITY_IOS
// using NotificationSamples.iOS;
// #endif
//
// public class NotificationsTester : MonoBehaviour
// {
//     public const string ChannelId = "game_channel_0";
//
//     // Default filename for notifications serializer
//     private const string DefaultFilename = "notifications.bin";
//
//     // Flag set when we're in the foreground
//     private bool m_InForeground = true;
//
//     [SerializeField, Tooltip("The operating mode for the notifications manager.")]
//     private GameNotificationsManager.OperatingMode
//         mode = GameNotificationsManager.OperatingMode.QueueClearAndReschedule;
//
//     // Minimum amount of time that a notification should be into the future before it's queued when we background.
//     private static readonly TimeSpan MinimumNotificationTime = new TimeSpan(0, 0, 2);
//
//     /// <summary>
//     /// Gets whether this manager has been initialized.
//     /// </summary>
//     public bool Initialized { get; private set; }
//
//     /// <summary>
//     /// Gets the implementation of the notifications for the current platform;
//     /// </summary>
//     public IGameNotificationsPlatform Platform { get; private set; }
//
//     /// <summary>
//     /// Event fired when a scheduled local notification is delivered while the app is in the foreground.
//     /// </summary>
//     public event Action<PendingNotification> LocalNotificationDelivered;
//
//     /// <summary>
//     /// Gets whether this manager automatically increments badge numbers.
//     /// </summary>
//     public bool AutoBadging => autoBadging;
//
//     /// <summary>
//     /// Event fired when a queued local notification is cancelled because the application is in the foreground
//     /// when it was meant to be displayed.
//     /// </summary>
//     /// <seealso cref="GameNotificationsManager.OperatingMode.Queue"/>
//     public event Action<PendingNotification> LocalNotificationExpired;
//
//     /// <summary>
//     /// Gets a collection of notifications that are scheduled or queued.
//     /// </summary>
//     public List<PendingNotification> PendingNotifications { get; private set; }
//
//     [SerializeField] protected Transform             pendingEventParent;
//
//     [SerializeField, Tooltip(
//          "Check to make the notifications manager automatically set badge numbers so that they increment.\n" +
//          "Schedule notifications with no numbers manually set to make use of this feature.")]
//     private bool autoBadging = true;
//
//     // Update pending notifications in the next update.
//     private bool m_UpdatePendingNotifications;
//
//     /// <summary>
//     /// Gets or sets the serializer to use to save pending notifications to disk if we're in
//     /// <see cref="GameNotificationsManager.OperatingMode.RescheduleAfterClearing"/> mode.
//     /// </summary>
//     public IPendingNotificationsSerializer Serializer { get; set; }
//
//     /// <summary>
//     /// Queue a notification with the given parameters.
//     /// </summary>
//     /// <param name="_Title">The title for the notification.</param>
//     /// <param name="_Body">The body text for the notification.</param>
//     /// <param name="_DeliveryTime">The time to deliver the notification.</param>
//     /// <param name="_BadgeNumber">The optional badge number to display on the application icon.</param>
//     /// <param name="_Reschedule">
//     /// Whether to reschedule the notification if foregrounding and the notification hasn't yet been shown.
//     /// </param>
//     /// <param name="_ChannelId">Channel ID to use. If this is null/empty then it will use the default ID. For Android
//     /// the channel must be registered in <see cref="GameNotificationsManager.Initialize"/>.</param>
//     /// <param name="_SmallIcon">Notification small icon.</param>
//     /// <param name="_LargeIcon">Notification large icon.</param>
//     public void SendNotification(
//         string   _Title,
//         string   _Body,
//         DateTime _DeliveryTime,
//         int?     _BadgeNumber = null,
//         bool     _Reschedule  = false,
//         string   _ChannelId   = null,
//         string   _SmallIcon   = null,
//         string   _LargeIcon   = null)
//     {
//         IGameNotification notification = CreateNotification();
//
//         if (notification == null)
//         {
//             return;
//         }
//
//         notification.Title = _Title;
//         notification.Body = _Body;
//         notification.Group = !string.IsNullOrEmpty(_ChannelId) ? _ChannelId : ChannelId;
//         notification.DeliveryTime = _DeliveryTime;
//         notification.SmallIcon = _SmallIcon;
//         notification.LargeIcon = _LargeIcon;
//         if (_BadgeNumber != null)
//         {
//             notification.BadgeNumber = _BadgeNumber;
//         }
//
//         PendingNotification notificationToDisplay = ScheduleNotification(notification);
//         notificationToDisplay.Reschedule = _Reschedule;
//         m_UpdatePendingNotifications = true;
//
//         QueueEvent($"Queued event with ID \"{notification.Id}\" at time {_DeliveryTime:HH:mm}");
//     }
//
//     /// <summary>
//     /// Creates a new notification object for the current platform.
//     /// </summary>
//     /// <returns>The new notification, ready to be scheduled, or null if there's no valid platform.</returns>
//     /// <exception cref="InvalidOperationException"><see cref="Initialize"/> has not been called.</exception>
//     public IGameNotification CreateNotification()
//     {
//         if (!Initialized)
//         {
//             throw new InvalidOperationException("Must call Initialize() first.");
//         }
//
//         return Platform?.CreateNotification();
//     }
//
//     /// <summary>
//     /// Initialize the notifications manager.
//     /// </summary>
//     /// <param name="_Channels">An optional collection of channels to register, for Android</param>
//     /// <exception cref="InvalidOperationException"><see cref="Initialize"/> has already been called.</exception>
//     public void Initialize(params GameNotificationChannel[] _Channels)
//     {
//         if (Initialized)
//         {
//             throw new InvalidOperationException("NotificationsManager already initialized.");
//         }
//
//         Initialized = true;
//
// #if UNITY_ANDROID
//             Platform = new AndroidNotificationsPlatform();
//
//             // Register the notification channels
//             var doneDefault = false;
//             foreach (GameNotificationChannel notificationChannel in _Channels)
//             {
//                 if (!doneDefault)
//                 {
//                     doneDefault = true;
//                     ((AndroidNotificationsPlatform)Platform).DefaultChannelId = notificationChannel.Id;
//                 }
//
//                 long[] vibrationPattern = null;
//                 if (notificationChannel.VibrationPattern != null)
//                     vibrationPattern = notificationChannel.VibrationPattern.Select(v => (long)v).ToArray();
//
//                 // Wrap channel in Android object
//                 var androidChannel = new AndroidNotificationChannel(notificationChannel.Id, notificationChannel.Name,
//                     notificationChannel.Description,
//                     (Importance)notificationChannel.Style)
//                 {
//                     CanBypassDnd = notificationChannel.HighPriority,
//                     CanShowBadge = notificationChannel.ShowsBadge,
//                     EnableLights = notificationChannel.ShowLights,
//                     EnableVibration = notificationChannel.Vibrates,
//                     LockScreenVisibility = (LockScreenVisibility)notificationChannel.Privacy,
//                     VibrationPattern = vibrationPattern
//                 };
//
//                 AndroidNotificationCenter.RegisterNotificationChannel(androidChannel);
//             }
// #elif UNITY_IOS
//         Platform = new iOSNotificationsPlatform();
// #endif
//
//         if (Platform == null)
//         {
//             return;
//         }
//
//         PendingNotifications = new List<PendingNotification>();
//         Platform.NotificationReceived += OnNotificationReceived;
//
//         // Check serializer
//         if (Serializer == null)
//         {
//             Serializer = new DefaultSerializer(Path.Combine(Application.persistentDataPath, DefaultFilename));
//         }
//
//         OnForegrounding();
//     }
//
//     /// <summary>
//     /// Event fired by <see cref="Platform"/> when a notification is received.
//     /// </summary>
//     private void OnNotificationReceived(IGameNotification _DeliveredNotification)
//     {
//         // Ignore for background messages (this happens on Android sometimes)
//         if (!m_InForeground)
//         {
//             return;
//         }
//
//         // Find in pending list
//         int deliveredIndex =
//             PendingNotifications.FindIndex(_ScheduledNotification =>
//                 _ScheduledNotification.Notification.Id == _DeliveredNotification.Id);
//         if (deliveredIndex >= 0)
//         {
//             LocalNotificationDelivered?.Invoke(PendingNotifications[deliveredIndex]);
//
//             PendingNotifications.RemoveAt(deliveredIndex);
//         }
//     }
//
//     /// <summary>
//     /// Schedules a notification to be delivered.
//     /// </summary>
//     /// <param name="_Notification">The notification to deliver.</param>
//     public PendingNotification ScheduleNotification(IGameNotification _Notification)
//     {
//         if (!Initialized)
//         {
//             throw new InvalidOperationException("Must call Initialize() first.");
//         }
//
//         if (_Notification == null || Platform == null)
//         {
//             return null;
//         }
//
//         // If we queue, don't schedule immediately.
//         // Also immediately schedule non-time based deliveries (for iOS)
//         if ((mode & GameNotificationsManager.OperatingMode.Queue) != GameNotificationsManager.OperatingMode.Queue ||
//             _Notification.DeliveryTime == null)
//         {
//             Platform.ScheduleNotification(_Notification);
//         }
//         else if (!_Notification.Id.HasValue)
//         {
//             // Generate an ID for items that don't have one (just so they can be identified later)
//             int id = Math.Abs(DateTime.Now.ToString("yyMMddHHmmssffffff").GetHashCode());
//             _Notification.Id = id;
//         }
//
//         // Register pending notification
//         var result = new PendingNotification(_Notification);
//         PendingNotifications.Add(result);
//
//         return result;
//     }
//
//     private void QueueEvent(string _EventText)
//     {
//         Dbg.Log($"Queueing event with text \"{_EventText}\"");
//
//     }
//
//     // Clear foreground notifications and reschedule stuff from a file
//     private void OnForegrounding()
//     {
//         PendingNotifications.Clear();
//
//         Platform.OnForeground();
//
//         // Deserialize saved items
//         IList<IGameNotification> loaded = Serializer?.Deserialize(Platform);
//
//         // Foregrounding
//         if ((mode & GameNotificationsManager.OperatingMode.ClearOnForegrounding) ==
//             GameNotificationsManager.OperatingMode.ClearOnForegrounding)
//         {
//             // Clear on foregrounding
//             Platform.CancelAllScheduledNotifications();
//
//             // Only reschedule in reschedule mode, and if we loaded any items
//             if (loaded == null ||
//                 (mode & GameNotificationsManager.OperatingMode.RescheduleAfterClearing) !=
//                 GameNotificationsManager.OperatingMode.RescheduleAfterClearing)
//             {
//                 return;
//             }
//
//             // Reschedule notifications from deserialization
//             foreach (IGameNotification savedNotification in loaded)
//             {
//                 if (savedNotification.DeliveryTime > DateTime.Now)
//                 {
//                     PendingNotification pendingNotification = ScheduleNotification(savedNotification);
//                     pendingNotification.Reschedule = true;
//                 }
//             }
//         }
//         else
//         {
//             // Just create PendingNotification wrappers for all deserialized items.
//             // We're not rescheduling them because they were not cleared
//             if (loaded == null)
//             {
//                 return;
//             }
//
//             foreach (IGameNotification savedNotification in loaded)
//             {
//                 if (savedNotification.DeliveryTime > DateTime.Now)
//                 {
//                     PendingNotifications.Add(new PendingNotification(savedNotification));
//                 }
//             }
//         }
//     }
//
//     /// <summary>
//     /// Check pending list for expired notifications, when in queue mode.
//     /// </summary>
//     protected virtual void Update()
//     {
//         if (PendingNotifications == null || !PendingNotifications.Any()
//                                          || (mode & GameNotificationsManager.OperatingMode.Queue) !=
//                                          GameNotificationsManager.OperatingMode.Queue)
//         {
//             return;
//         }
//     
//         // Check each pending notification for expiry, then remove it
//         for (int i = PendingNotifications.Count - 1; i >= 0; --i)
//         {
//             PendingNotification queuedNotification = PendingNotifications[i];
//             DateTime? time = queuedNotification.Notification.DeliveryTime;
//             if (time != null && time < DateTime.Now)
//             {
//                 PendingNotifications.RemoveAt(i);
//                 LocalNotificationExpired?.Invoke(queuedNotification);
//             }
//         }
//     }
//
//     /// <summary>
//     /// Clean up platform object if necessary
//     /// </summary>
//     protected virtual void OnDestroy()
//     {
//         if (Platform == null)
//             return;
//         Platform.NotificationReceived -= OnNotificationReceived;
//         if (Platform is IDisposable disposable)
//             disposable.Dispose();
//         m_InForeground = false;
//     }
//
//     /// <summary>
//     /// Respond to application foreground/background events.
//     /// </summary>
//     protected void OnApplicationFocus(bool _HasFocus)
//     {
//         if (Platform == null || !Initialized)
//         {
//             return;
//         }
//     
//         m_InForeground = _HasFocus;
//     
//         if (_HasFocus)
//         {
//             OnForegrounding();
//     
//             return;
//         }
//     
//         Platform.OnBackground();
//     
//         // Backgrounding
//         // Queue future dated notifications
//         if ((mode & GameNotificationsManager.OperatingMode.Queue) == GameNotificationsManager.OperatingMode.Queue)
//         {
//             // Filter out past events
//             for (var i = PendingNotifications.Count - 1; i >= 0; i--)
//             {
//                 PendingNotification pendingNotification = PendingNotifications[i];
//                 // Ignore already scheduled ones
//                 if (pendingNotification.Notification.Scheduled)
//                 {
//                     continue;
//                 }
//     
//                 // If a non-scheduled notification is in the past (or not within our threshold)
//                 // just remove it immediately
//                 if (pendingNotification.Notification.DeliveryTime != null &&
//                     pendingNotification.Notification.DeliveryTime - DateTime.Now < MinimumNotificationTime)
//                 {
//                     PendingNotifications.RemoveAt(i);
//                 }
//             }
//     
//             // Sort notifications by delivery time, if no notifications have a badge number set
//             bool noBadgeNumbersSet =
//                 PendingNotifications.All(_Notification => _Notification.Notification.BadgeNumber == null);
//     
//             if (noBadgeNumbersSet && AutoBadging)
//             {
//                 PendingNotifications.Sort((_A, _B) =>
//                 {
//                     if (!_A.Notification.DeliveryTime.HasValue)
//                     {
//                         return 1;
//                     }
//     
//                     if (!_B.Notification.DeliveryTime.HasValue)
//                     {
//                         return -1;
//                     }
//     
//                     return _A.Notification.DeliveryTime.Value.CompareTo(_B.Notification.DeliveryTime.Value);
//                 });
//     
//                 // Set badge numbers incrementally
//                 var badgeNum = 1;
//                 foreach (PendingNotification pendingNotification in PendingNotifications)
//                 {
//                     if (pendingNotification.Notification.DeliveryTime.HasValue &&
//                         !pendingNotification.Notification.Scheduled)
//                     {
//                         pendingNotification.Notification.BadgeNumber = badgeNum++;
//                     }
//                 }
//             }
//     
//             for (int i = PendingNotifications.Count - 1; i >= 0; i--)
//             {
//                 PendingNotification pendingNotification = PendingNotifications[i];
//                 // Ignore already scheduled ones
//                 if (pendingNotification.Notification.Scheduled)
//                 {
//                     continue;
//                 }
//     
//                 // Schedule it now
//                 Platform.ScheduleNotification(pendingNotification.Notification);
//             }
//     
//             // Clear badge numbers again (for saving)
//             if (noBadgeNumbersSet && AutoBadging)
//             {
//                 foreach (PendingNotification pendingNotification in PendingNotifications)
//                 {
//                     if (pendingNotification.Notification.DeliveryTime.HasValue)
//                     {
//                         pendingNotification.Notification.BadgeNumber = null;
//                     }
//                 }
//             }
//         }
//     
//         // Calculate notifications to save
//         var notificationsToSave = new List<PendingNotification>(PendingNotifications.Count);
//         foreach (PendingNotification pendingNotification in PendingNotifications)
//         {
//             // If we're in clear mode, add nothing unless we're in rescheduling mode
//             // Otherwise add everything
//             if ((mode & GameNotificationsManager.OperatingMode.ClearOnForegrounding) ==
//                 GameNotificationsManager.OperatingMode.ClearOnForegrounding)
//             {
//                 if ((mode & GameNotificationsManager.OperatingMode.RescheduleAfterClearing) !=
//                     GameNotificationsManager.OperatingMode.RescheduleAfterClearing)
//                 {
//                     continue;
//                 }
//     
//                 // In reschedule mode, add ones that have been scheduled, are marked for
//                 // rescheduling, and that have a time
//                 if (pendingNotification.Reschedule &&
//                     pendingNotification.Notification.Scheduled &&
//                     pendingNotification.Notification.DeliveryTime.HasValue)
//                 {
//                     notificationsToSave.Add(pendingNotification);
//                 }
//             }
//             else
//             {
//                 // In non-clear mode, just add all scheduled notifications
//                 if (pendingNotification.Notification.Scheduled)
//                 {
//                     notificationsToSave.Add(pendingNotification);
//                 }
//             }
//         }
//     
//         // Save to disk
//         Serializer.Serialize(notificationsToSave);
//     }
//
//     private void Start()
//     {
//         var channel = new GameNotificationChannel(
//             ChannelId, Application.productName + " main channel", "Generic notifications");
//         Initialize(channel);
//     }
//
//     public void ButtonSendNotification()
//     {
//         var dt = DateTime.Now.AddSeconds(20);
//         SendNotification(
//             "Test title", 
//             "Test body", 
//             dt, 
//             2, 
//             false,
//             ChannelId, 
//             "main_icon",
//             "main_icon_large");
//     }
// }