namespace Settings
{
    public class SoundSetting : ISetting
    {
        public SettingType Type => SettingType.OnOff;

        public void Set(object _Parameter)
        {
            bool volumeOn = (bool) _Parameter;
        
            //TODO set sound on/off
        }
    }
}
