using System.Collections.Generic;
using Entities;
using UnityEngine;
using Utils;

namespace Settings
{
    public class SoundSetting : ISetting
    {
        public string Name => "Sound";
        public SettingType Type => SettingType.OnOff;
        public List<string> Values => null;
        public object Min => null;
        public object Max => null;

        public object Get()
        {
            return SaveUtils.GetValue<bool>(SaveKey.SettingSoundOn);
        }
        
        public void Put(object _Parameter)
        {
            bool volumeOn = (bool) _Parameter;
            SoundManager.Instance.SwitchSound(volumeOn);
            SoundManager.Instance.SwitchSoundInActualClips(volumeOn);
            Debug.Log(volumeOn.ToString());
        }
    }
}
