using Common.Constants;
using Common.Entities;
using Common.Managers.Analytics;
using UnityEngine.Events;

namespace Common.Settings
{
    public interface ISoundSetting : ISetting<bool> { }
    
    public class SoundSetting : SettingBase<bool>, ISoundSetting
    {
        private IAnalyticsManager AnalyticsManager { get; }

        public SoundSetting(IAnalyticsManager _AnalyticsManager)
        {
            AnalyticsManager = _AnalyticsManager;
        }
        
        public override SaveKey<bool>     Key          => SaveKeysCommon.SettingSoundOn;
        public override string            TitleKey     => "Sound";
        public override ESettingLocation  Location     => ESettingLocation.MiniButtons;
        public override ESettingType      Type         => ESettingType.OnOff;
        public override string            SpriteOnKey  => "setting_sound_on";
        public override string            SpriteOffKey => "setting_sound_off";

        public override void Put(bool _Value)
        {
            AnalyticsManager.SendAnalytic(_Value ? 
                AnalyticIds.EnableSoundButtonPressed : AnalyticIds.DisableSoundButtonPressed);
            base.Put(_Value);
        }
    }
}
