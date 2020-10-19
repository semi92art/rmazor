namespace Settings
{
    public class LanguageSetting : ISetting
    {
        public SettingType Type => SettingType.DropSelection;

        public void Set(object _Parameter)
        {
            Language lang = (Language) _Parameter;
            
            //TODO set language 
        }
    }
}