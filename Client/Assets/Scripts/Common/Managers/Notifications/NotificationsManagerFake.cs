using System;
using System.Collections.Generic;
using Common.Helpers;

namespace Common.Managers.Notifications
{
    public class NotificationsManagerFake : InitBase, INotificationsManager
    {
        public event Action<PendingNotification> LocalNotificationDelivered;
        public event Action<PendingNotification> LocalNotificationExpired;

        public ENotificationsOperatingMode OperatingMode        { get; set; } = ENotificationsOperatingMode.NoQueue;
        public List<PendingNotification>   PendingNotifications => new List<PendingNotification>();
        public int?                        LastNotificationsCountToReschedule { get; set; }

        public void EnableNotifications(bool _Enable)
        {
            Dbg.LogWarning("Enabling/disabling notifications is available on device only.");
        }

        public void SendNotification(
            string   _Title,
            string   _Body,
            DateTime _DeliveryTime,
            int?     _BadgeNumber = null,
            bool     _Reschedule  = false,
            string   _ChannelId   = null,
            string   _SmallIcon   = null,
            string   _LargeIcon   = null) { }

        public void ClearAllNotifications() { }
    }
}