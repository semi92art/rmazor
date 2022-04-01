using Common.Constants;
using Common.Entities;
using Common.Managers.Analytics;
using UnityEngine.Events;

namespace Common.Settings
{
    public interface IHapticsSetting : ISetting<bool> { }
    
    public class HapticsSetting : SettingBase<bool>, IHapticsSetting
    {
        private IAnalyticsManager AnalyticsManager { get; }

        public HapticsSetting(IAnalyticsManager _AnalyticsManager)
        {
            AnalyticsManager = _AnalyticsManager;
        }
        
        public override UnityAction<bool> OnValueSet   { get; set; }
        public override SaveKey<bool>     Key          => SaveKeysCommon.SettingHapticsOn;
        public override string            TitleKey     => "Haptics";
        public override ESettingLocation  Location     => ESettingLocation.MiniButtons;
        public override ESettingType      Type         => ESettingType.OnOff;
        public override string            SpriteOnKey  => "setting_haptics_on";
        public override string            SpriteOffKey => "setting_haptics_off";

        public override void Put(bool _Value)
        {
            AnalyticsManager.SendAnalytic(_Value ? 
                AnalyticIds.EnableHapticsButtonPressed : AnalyticIds.DisableHapticsButtonPressed);
            base.Put(_Value);
        }
    }
}