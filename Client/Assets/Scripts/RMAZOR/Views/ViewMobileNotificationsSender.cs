using System;
using Common;
using mazing.common.Runtime;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Managers.Notifications;
using mazing.common.Runtime.Utils;
using UnityEngine;

namespace RMAZOR.Views
{
    public interface IViewMobileNotificationsSender : IInit { }
    
    public class ViewMobileNotificationsSender
        : InitBase,
          IViewMobileNotificationsSender
    {
        private IRemotePropertiesRmazor RemotePropertiesRmazor { get; }
        private INotificationsManager   NotificationsManager   { get; }
        private ILocalizationManager    LocalizationManager    { get; }

        public ViewMobileNotificationsSender(
            IRemotePropertiesRmazor _RemotePropertiesRmazor,
            INotificationsManager   _NotificationsManager,
            ILocalizationManager    _LocalizationManager)
        {
            RemotePropertiesRmazor = _RemotePropertiesRmazor;
            NotificationsManager   = _NotificationsManager;
            LocalizationManager    = _LocalizationManager;
        }
        
        public override void Init()
        {
            SendNotificationsOnInit();
            base.Init();
        }
        
        private void SendNotificationsOnInit()
        {
            if (!MazorCommonData.Release)
                return;
            if (RemotePropertiesRmazor.Notifications == null)
                return;
            var notMan = NotificationsManager;
            var notifications = RemotePropertiesRmazor.Notifications 
                                ?? DefaultNotificationsGetter.GetNotifications();
            var sessionsDict = SaveUtils.GetValue(SaveKeysRmazor.SessionCountByDays);
            bool needToSendNotifications = false;
            var today = DateTime.Now.Date;
            for (int iDay = 0; iDay < 5; iDay++)
            {
                var iDateTime = today - TimeSpan.FromDays(iDay);
                int sessionsCount = sessionsDict.GetSafe(iDateTime, out _);
                if (sessionsCount > 0)
                    continue;
                needToSendNotifications = true;
                break;
            }
            if (!needToSendNotifications)
                return;
            notMan.OperatingMode = ENotificationsOperatingMode.NoQueue;
            notMan.ClearAllNotifications();
            var currentLanguage = LocalizationManager.GetCurrentLanguage();
            foreach (var notification in notifications)
            {
                notMan.SendNotification(
                    notification.Title[currentLanguage], 
                    notification.Body[currentLanguage], 
                    notification.Span,
                    _Reschedule: Application.platform == RuntimePlatform.Android,
                    _SmallIcon: "small_notification_icon",
                    _LargeIcon: "large_notification_icon");
            }
        }
    }
}