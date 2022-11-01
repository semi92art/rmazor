using System;
using System.Text;
using UnityEngine;
#if UNITY_ANDROID
using SA.Android.App;
#endif

namespace Common.Managers.Notifications
{
    public class NotificationsManagerStanAssets : NotificationsManagerBase
    {
        #region constants

        private const string ChannelId = "rmazor1";
        
        #endregion
        
        #region nonpublic members
        
        private int    m_LastNotificationId;

        #endregion

        #region inject
        
        private IPrefabSetManager PrefabSetManager { get; }
        

        public NotificationsManagerStanAssets(IPrefabSetManager _PrefabSetManager)
        {
            PrefabSetManager = _PrefabSetManager;
        }

        #endregion

        #region api

        public override void Init()
        {
            InitListener();
            CreateChannelIfNotExist();
            ClearAllNotifications();
            base.Init();
        }

        public override void SendNotification(
            string   _Title,
            string   _Body,
            TimeSpan _TimeSpan,
            int?     _BadgeNumber = null,
            bool     _Reschedule  = false,
            string   _ChannelId   = null,
            string   _SmallIcon   = null,
            string   _LargeIcon   = null)
        {
#if UNITY_ANDROID
            var builder = new AN_NotificationCompat.Builder();
            builder.SetContentTitle(_Title);
            builder.SetContentText(_Body);
            builder.SetChanelId(ChannelId);
            if (!string.IsNullOrEmpty(_SmallIcon))
                builder.SetSmallIcon(_SmallIcon);
            var trigger = new AN_AlarmNotificationTrigger();
            trigger.SetDate(_TimeSpan);
            int id = ++m_LastNotificationId; 
            var request = new AN_NotificationRequest(id, builder, trigger);
            AN_NotificationManager.Schedule(request);
            Dbg.Log("Sending notification: \n" +
                    "Title: " + _Title + ", \n" +
                    "Body: " + _Body + ", \n" +
                    "Span: " + _TimeSpan);
#endif
        }

        public override void ClearAllNotifications()
        {
#if UNITY_ANDROID
            AN_NotificationManager.UnscheduleAll();
#endif
        }

        #endregion

        #region nonpublic methods

        private void InitListener()
        {
#if UNITY_ANDROID
            AN_NotificationManager.OnNotificationReceived.AddSafeListener(this, (_Request) => 
            {
                Dbg.Log("request.Identifier: " + _Request.Identifier);
                Dbg.Log("User has opened the local notification request with info: " 
                          + JsonUtility.ToJson(_Request));
            });
#endif
        }

        private static void CreateChannelIfNotExist()
        {
#if UNITY_ANDROID
            var channel = AN_NotificationManager.GetNotificationChannel(ChannelId);
            if (channel != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Notification channel with id {ChannelId} exists:");
                sb.AppendLine("Id: " + channel.Id);
                sb.AppendLine("Name: " + channel.Name);
                sb.AppendLine("Description: " + channel.Description);
                sb.AppendLine("Importance: " + channel.Importance);
                sb.AppendLine("Sound: " + channel.Sound);
                sb.AppendLine("Can show bridge: " + channel.CanShowBadge);
                Dbg.LogWarning(sb.ToString());
                return;
            }
            const string name = "rmazor_name";
            const string description = "rmazor_descr";
            var importance = AN_NotificationManager.Importance.DEFAULT;
            channel = new AN_NotificationChannel(ChannelId, name, importance);
            channel.Description = description;
            AN_NotificationManager.CreateNotificationChannel(channel);
#endif
        }

        #endregion
    }
}