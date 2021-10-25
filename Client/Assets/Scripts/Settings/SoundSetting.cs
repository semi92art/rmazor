using Entities;
using Lean.Localization;

namespace Settings
{
    public interface ISoundSetting : ISetting<bool> { }
    
    public class SoundSetting : SettingBase<bool>, ISoundSetting
    {
        private IManagersGetter Managers { get; }
        
        public SoundSetting(IManagersGetter _Managers)
        {
            Managers = _Managers;
        }

        public override SaveKey Key => SaveKey.SettingSoundOn;
        public override string TitleKey => "Sound";
        public override ESettingLocation Location => ESettingLocation.MiniButtons;
        public override ESettingType Type => ESettingType.OnOff;
        public override string SpriteOnKey => "setting_sound_on";
        public override string SpriteOffKey => "setting_sound_off";

        public override void Put(bool _VolumeOn)
        {
            Managers.Notify(_SM => _SM.EnableSound(_VolumeOn, "sound"));
            base.Put(_VolumeOn);
        }
    }
}
