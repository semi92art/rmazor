using System;
using Common;
using mazing.common.Runtime;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
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

        public ViewMobileNotificationsSender(
            IRemotePropertiesRmazor _RemotePropertiesRmazor,
            INotificationsManager   _NotificationsManager)
        {
            RemotePropertiesRmazor = _RemotePropertiesRmazor;
            NotificationsManager   = _NotificationsManager;
        }
        
        public override void Init()
        {
            CommonUtils.DoOnInitializedEx(NotificationsManager, SendNotificationsOnInit);
            base.Init();
        }
        
        private void SendNotificationsOnInit()
        {
            if (!CommonDataMazor.Release)
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
            foreach (var notification in notifications)
            {
                notMan.SendNotification(
                    notification.Title[ELanguage.English], 
                    notification.Body[ELanguage.English], 
                    notification.Span,
                    _Reschedule: Application.platform == RuntimePlatform.Android,
                    _SmallIcon: "small_notification_icon");
            }
        }
    }
}