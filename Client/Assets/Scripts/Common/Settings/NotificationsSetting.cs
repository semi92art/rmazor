using Common.Entities;
using UnityEngine.Events;

namespace Common.Settings
{
    public interface INotificationSetting : ISetting<bool> { }
    
    public class NotificationsSetting : SettingBase<bool>, INotificationSetting
    {
        public override SaveKey<bool>    Key          => SaveKeysCommon.SettingNotificationsOn;
        public override string           TitleKey     => "Notifications";
        public override ESettingLocation Location     => ESettingLocation.MiniButtons;
        public override ESettingType     Type         => ESettingType.OnOff;
        public override string           SpriteOnKey  => "setting_notifications_on";
        public override string           SpriteOffKey => "setting_notifications_off";
    }
}