using System.Collections.Generic;
using Constants;
using Constants.NotifyMessages;
using Entities;
using Lean.Localization;
using Ticker;
using Utils;

namespace Settings
{
    public class SoundSetting : ISetting
    {
        private IGameObservable GameObservable { get; }
        public string Name => LeanLocalization.GetTranslationText("Sound");
        public SettingType Type => SettingType.OnOff;
        public List<string> Values => null;
        public object Min => null;
        public object Max => null;

        public SoundSetting(IGameObservable _GameObservable)
        {
            GameObservable = _GameObservable;
        }
        
        public object Get()
        {
            return SaveUtils.GetValue<bool>(SaveKey.SettingSoundOn);
        }
        
        public void Put(object _Parameter)
        {
            bool volumeOn = (bool) _Parameter;
            GameObservable.Notify(SoundNotifyMessages.SwitchSoundSetting, volumeOn);
            Dbg.Log(volumeOn.ToString());
        }
    }
}
