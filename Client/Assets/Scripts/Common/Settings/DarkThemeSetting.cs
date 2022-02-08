using Common;
using Common.Entities;
using UnityEngine.Events;

namespace Settings
{
    public interface IDarkThemeSetting : ISetting<bool> { }
    
    public class DarkThemeSetting : SettingBase<bool>, IDarkThemeSetting
    {
        public override UnityAction<bool> OnValueSet   { get; set; }
        public override SaveKey<bool>     Key          => SaveKeysCommon.DarkTheme;
        public override string            TitleKey     => "dark_theme";
        public override ESettingLocation  Location     => ESettingLocation.MiniButtons;
        public override ESettingType      Type         => ESettingType.OnOff;
        public override string            SpriteOnKey  => "dark_theme_on";
        public override string            SpriteOffKey => "dark_theme_off";
    }
}