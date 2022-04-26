using System;
using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Enums;
using UnityEngine.Events;

namespace Common.Settings
{
    public interface ILanguageSetting : ISetting<ELanguage>
    {
        Func<ELanguage> GetValue { get; set; }
    }
    
    public class LanguageSetting : SettingBase<ELanguage>, ILanguageSetting
    {
        public          Func<ELanguage> GetValue { get; set; }
        public override SaveKey<ELanguage> Key => null;
        public override string                TitleKey   => "Language";
        public override ESettingLocation      Location   => ESettingLocation.Main;
        public override ESettingType          Type       => ESettingType.InPanelSelector;
        public override List<ELanguage>        Values     => Enum.GetValues(typeof(ELanguage)).Cast<ELanguage>().ToList();

        public override ELanguage Get()
        {
            return GetValue();
        }

        public override void Put(ELanguage _Value)
        {
            RaiseValueSetEvent(_Value);
        }
    }
}