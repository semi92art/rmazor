using Entities;
using UnityEngine.Events;

namespace Settings
{
    public interface IDebugSetting: ISetting<bool> { }
    
    public class DebugSetting : SettingBase<bool>, IDebugSetting
    {
        public override UnityAction<bool> OnValueSet { get; set; }
        public override SaveKey           Key        => SaveKey.DebugUtilsOn;
        public override string            TitleKey   => "Debug";
        public override ESettingLocation  Location   => ESettingLocation.Main;
        public override ESettingType      Type       => ESettingType.OnOff;
        
        public override void Put(bool _DebugOn)
        {
            base.Put(_DebugOn);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            OnValueSet?.Invoke(_DebugOn);
#endif
        }
    }
}