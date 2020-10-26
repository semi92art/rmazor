using System.Collections.Generic;
using Utils;

namespace Settings
{
    public class SoundSetting : ISetting
    {
        public string Name { get; }
        public SettingType Type => SettingType.OnOff;
        public List<string> Values => null;
        public object Min => null;
        public object Max => null;
        public SoundSetting()
        {
            Name = "Sound";
        }
        
        public object Get()
        {
            return SaveUtils.GetValue<bool>(SaveKey.SettingSoundOn);
        }
        
        public void Put(object _Parameter)
        {
            bool volumeOn = (bool) _Parameter;
            SaveUtils.PutValue(SaveKey.SettingSoundOn, volumeOn);
        }
    }
}
