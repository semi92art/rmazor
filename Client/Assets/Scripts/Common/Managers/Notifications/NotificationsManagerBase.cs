using System;
using Common.Helpers;

namespace Common.Managers.Notifications
{
    [Flags]
    public enum ENotificationsOperatingMode
    {
        NoQueue = 0x00,
        /// <summary>
        /// <para>
        /// Queue messages that are scheduled with this manager.
        /// No messages will be sent to the operating system until the application is backgrounded.
        /// </para>
        /// <para>
        /// If badge numbers are not set, will automatically increment them. This will only happen if NO badge numbers
        /// for pending notifications are ever set.
        /// </para>
        /// </summary>
        Queue = 0x01,

        /// <summary>
        /// When the application is foregrounded, clear all pending notifications.
        /// </summary>
        ClearOnForegrounding = 0x02,

        /// <summary>
        /// After clearing events, will put future ones back into the queue if they are marked with <see cref="PendingNotification.Reschedule"/>.
        /// </summary>
        /// <remarks>
        /// Only valid if <see cref="ClearOnForegrounding"/> is also set.
        /// </remarks>
        RescheduleAfterClearing = 0x04,

        /// <summary>
        /// Combines the behaviour of <see cref="Queue"/> and <see cref="ClearOnForegrounding"/>.
        /// </summary>
        QueueAndClear = Queue | ClearOnForegrounding,

        /// <summary>
        /// <para>
        /// Combines the behaviour of <see cref="Queue"/>, <see cref="ClearOnForegrounding"/> and
        /// <see cref="RescheduleAfterClearing"/>.
        /// </para>
        /// <para>
        /// Ensures that messages will never be displayed while the application is in the foreground.
        /// </para>
        /// </summary>
        QueueClearAndReschedule = Queue | ClearOnForegrounding | RescheduleAfterClearing,
    }
    
    public interface INotificationsManager : IInit
    {
        ENotificationsOperatingMode OperatingMode { get; set; }

        void SendNotification(
            string   _Title,
            string   _Body,
            TimeSpan _TimeSpan,
            int?     _BadgeNumber = null,
            bool     _Reschedule  = false,
            string   _ChannelId   = null,
            string   _SmallIcon   = null,
            string   _LargeIcon   = null);

        int? LastNotificationsCountToReschedule { get; set; }
        
        void ClearAllNotifications();
    }
    
    public abstract class NotificationsManagerBase : InitBase, INotificationsManager
    {
        public ENotificationsOperatingMode       OperatingMode { get; set; }

        public int? LastNotificationsCountToReschedule { get; set; }

        public abstract void SendNotification(
            string   _Title,
            string   _Body,
            TimeSpan _TimeSpan,
            int?     _BadgeNumber = null,
            bool     _Reschedule  = false,
            string   _ChannelId   = null,
            string   _SmallIcon   = null,
            string   _LargeIcon   = null);

        public abstract void ClearAllNotifications();
    }
}