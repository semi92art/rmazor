using System.Collections.Generic;
using System.Linq;
using Utils;

namespace Settings
{
    public class LanguageSetting : ISetting
    {
        public string Name { get; }
        public SettingType Type => SettingType.InPanelSelector;

        public List<string> Values => m_LangNames.Values.OrderBy(_Item => _Item).ToList();
        public object Min => null;
        public object Max => null;

        private Dictionary<Language, string> m_LangNames = new Dictionary<Language, string>
        {
            {Language.English, "English"},
            {Language.Portugal, "Portugués"},
            {Language.Spanish, "Español"},
            {Language.Russian, "Русский"}
        };

        public LanguageSetting()
        {
            Name = "Language";
        }
        
        public object Get()
        {
            return m_LangNames[SaveUtils.GetValue<Language>(SaveKey.SettingLanguage)];
        }
        
        public void Put(object _Parameter)
        {
            string langName = (string) _Parameter;
            Language lang = m_LangNames
                .ToList()
                .FirstOrDefault(_Kvp => _Kvp.Value == langName).Key;
            SaveUtils.PutValue(SaveKey.SettingLanguage, lang);
        }
    }
}