using Common.Entities;
using UnityEngine.Events;

namespace Common.Settings
{
    public interface ISoundSetting : ISetting<bool> { }
    
    public class SoundSetting : SettingBase<bool>, ISoundSetting
    {
        public override UnityAction<bool> OnValueSet   { get; set; }
        public override SaveKey<bool>     Key          => SaveKeysCommon.SettingSoundOn;
        public override string            TitleKey     => "Sound";
        public override ESettingLocation  Location     => ESettingLocation.MiniButtons;
        public override ESettingType      Type         => ESettingType.OnOff;
        public override string            SpriteOnKey  => "setting_sound_on";
        public override string            SpriteOffKey => "setting_sound_off";
    }
}
