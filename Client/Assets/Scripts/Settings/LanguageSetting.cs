using System;
using System.Collections.Generic;
using System.Linq;
using Entities;

namespace Settings
{
    public interface ILanguageSetting : ISetting<Language> { }
    
    public class LanguageSetting : SettingBase<Language>, ILanguageSetting
    {
        private ILocalizationManager LocalizationManager { get; }

        public LanguageSetting(ILocalizationManager _LocalizationManager)
        {
            LocalizationManager = _LocalizationManager;
        }

        public override SaveKey Key => null;
        public override string TitleKey => "Language";
        public override ESettingLocation Location => ESettingLocation.Main;
        public override ESettingType Type => ESettingType.InPanelSelector;
        public override List<Language> Values => Enum.GetValues(typeof(Language)).Cast<Language>().ToList();

        public override Language Get()
        {
            return LocalizationManager.GetCurrentLanguage();
        }

        public override void Put(Language _Language)
        {
            LocalizationManager.SetLanguage(_Language);
        }
    }
}