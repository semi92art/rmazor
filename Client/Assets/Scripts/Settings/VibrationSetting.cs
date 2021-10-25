using Entities;

namespace Settings
{
    public interface IVibrationSetting : ISetting<bool> { }
    
    public class VibrationSetting : SettingBase<bool>, IVibrationSetting
    {
        public override SaveKey Key => SaveKey.SettingVibrationOn;
        public override string TitleKey => "Vibration";
        public override ESettingLocation Location => ESettingLocation.MiniButtons;
        public override ESettingType Type => ESettingType.OnOff;
        public override string SpriteOnKey => "setting_vibration_on";
        public override string SpriteOffKey => "setting_vibration_off";
    }
}