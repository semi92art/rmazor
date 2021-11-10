using Controllers;
using Entities;
using Lean.Localization;

namespace Settings
{
    public interface IMusicSetting : ISetting<bool> { }
    
    public class MusicSetting : SettingBase<bool>, IMusicSetting
    {
        private IAudioManager AudioManager { get; }

        public MusicSetting(IAudioManager _AudioManager)
        {
            AudioManager = _AudioManager;
        }

        public override SaveKey Key => SaveKey.SettingSoundOn;
        public override string TitleKey => "Music";
        public override ESettingLocation Location => ESettingLocation.MiniButtons;
        public override ESettingType Type => ESettingType.OnOff;
        public override string SpriteOnKey => "setting_music_on";
        public override string SpriteOffKey => "setting_music_off";

        public override void Put(bool _MusicOn)
        {
            AudioManager.EnableAudio(_MusicOn, EAudioClipType.Music);
            base.Put(_MusicOn);
        }
    }
}