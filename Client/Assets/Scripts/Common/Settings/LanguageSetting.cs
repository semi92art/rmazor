using System;
using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Enums;
using UnityEngine.Events;

namespace Common.Settings
{
    public interface ILanguageSetting : ISetting<Language>
    {
        Func<Language> GetValue { get; set; }
    }
    
    public class LanguageSetting : SettingBase<Language>, ILanguageSetting
    {
        public          Func<Language> GetValue { get; set; }
        public override UnityAction<Language> OnValueSet { get; set; }
        public override SaveKey<Language> Key => null;
        public override string                TitleKey   => "Language";
        public override ESettingLocation      Location   => ESettingLocation.Main;
        public override ESettingType          Type       => ESettingType.InPanelSelector;
        public override List<Language>        Values     => Enum.GetValues(typeof(Language)).Cast<Language>().ToList();

        public override Language Get()
        {
            return GetValue();
        }

        public override void Put(Language _Value)
        {
            OnValueSet?.Invoke(_Value);
        }
    }
}