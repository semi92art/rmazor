using Entities;
using UnityEngine.Events;

namespace Settings
{
    public interface ISoundSetting : ISetting<bool> { }
    
    public class SoundSetting : SettingBase<bool>, ISoundSetting
    {
        public override UnityAction<bool> OnValueSet   { get; set; }
        public override SaveKey<bool>     Key          => SaveKeys.SettingSoundOn;
        public override string            TitleKey     => "Sound";
        public override ESettingLocation  Location     => ESettingLocation.MiniButtons;
        public override ESettingType      Type         => ESettingType.OnOff;
        public override string            SpriteOnKey  => "setting_sound_on";
        public override string            SpriteOffKey => "setting_sound_off";

        public override void Put(bool _VolumeOn)
        {
            base.Put(_VolumeOn);
            OnValueSet?.Invoke(_VolumeOn);
        }

        
    }
}
