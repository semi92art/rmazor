﻿using Entities;

namespace Settings
{
    public interface INotificationSetting : ISetting<bool> { }
    
    public class NotificationsSetting : SettingBase<bool>, INotificationSetting
    {
        public override SaveKey Key => SaveKey.SettingNotificationsOn;
        public override string TitleKey => "Notifications";
        public override ESettingLocation Location => ESettingLocation.MiniButtons;
        public override ESettingType Type => ESettingType.OnOff;
        public override string SpriteOnKey { get; }
        public override string SpriteOffKey { get; }
    }
}