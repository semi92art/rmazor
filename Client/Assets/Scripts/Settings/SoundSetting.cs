using System.Collections.Generic;
using Constants;
using Entities;
using Lean.Localization;
using Ticker;
using Utils;

namespace Settings
{
    public class SoundSetting : GameObservable, ISetting
    {
        public string Name => LeanLocalization.GetTranslationText("Sound");
        public SettingType Type => SettingType.OnOff;
        public List<string> Values => null;
        public object Min => null;
        public object Max => null;

        public SoundSetting(IUITicker _UITicker) : base(_UITicker)
        {
        }
        
        public object Get()
        {
            return SaveUtils.GetValue<bool>(SaveKey.SettingSoundOn);
        }
        
        public void Put(object _Parameter)
        {
            bool volumeOn = (bool) _Parameter;
            Notify(this, CommonNotifyMessages.UiButtonClick, volumeOn);
            Dbg.Log(volumeOn.ToString());
        }
    }
}
