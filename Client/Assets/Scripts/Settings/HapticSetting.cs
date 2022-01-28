using Common.Entities;
using RMAZOR;
using UnityEngine.Events;

namespace Settings
{
    public interface IHapticsSetting : ISetting<bool> { }
    
    public class HapticsSetting : SettingBase<bool>, IHapticsSetting
    {
        public override UnityAction<bool> OnValueSet   { get; set; }
        public override SaveKey<bool>     Key          => SaveKeys.SettingHapticsOn;
        public override string            TitleKey     => "Haptics";
        public override ESettingLocation  Location     => ESettingLocation.MiniButtons;
        public override ESettingType      Type         => ESettingType.OnOff;
        public override string            SpriteOnKey  => "setting_haptics_on";
        public override string            SpriteOffKey => "setting_haptics_off";
    }
}