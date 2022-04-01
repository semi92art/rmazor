using Common.Constants;
using Common.Entities;
using Common.Managers.Analytics;
using UnityEngine.Events;

namespace Common.Settings
{
    public interface IMusicSetting : ISetting<bool> { }
    
    public class MusicSetting : SettingBase<bool>, IMusicSetting
    {
        private IAnalyticsManager AnalyticsManager { get; }

        public MusicSetting(IAnalyticsManager _AnalyticsManager)
        {
            AnalyticsManager = _AnalyticsManager;
        }
        
        public override UnityAction<bool> OnValueSet   { get; set; }
        public override SaveKey<bool>     Key          => SaveKeysCommon.SettingMusicOn;
        public override string            TitleKey     => "Music";
        public override ESettingLocation  Location     => ESettingLocation.MiniButtons;
        public override ESettingType      Type         => ESettingType.OnOff;
        public override string            SpriteOnKey  => "setting_music_on";
        public override string            SpriteOffKey => "setting_music_off";

        public override void Put(bool _Value)
        {
            AnalyticsManager.SendAnalytic(_Value ? 
                AnalyticIds.EnableMusicButtonPressed : AnalyticIds.DisableMusicButtonPressed);
            base.Put(_Value);
        }
    }
}