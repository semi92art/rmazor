using Common.Entities;
using Common.Helpers;
using Common.Utils;
using UnityEngine.Events;

namespace Common.Settings
{
    public interface IDebugSetting : ISetting<bool> { }
    
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DebugSetting : SettingBase<bool>, IDebugSetting
    {
        private CommonGameSettings Settings { get; }

        public DebugSetting(CommonGameSettings _Settings)
        {
            Settings = _Settings;
        }
        
        public override UnityAction<bool> OnValueSet { get; set; }
        public override SaveKey<bool>     Key        => SaveKeysCommon.DebugUtilsOn;
        public override string            TitleKey   => "Debug";
        public override ESettingLocation  Location   => ESettingLocation.Main;
        public override ESettingType      Type       => ESettingType.OnOff;
        
        public override void Put(bool _Value)
        {
            SaveUtils.PutValue(Key, _Value);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            OnValueSet?.Invoke(_Value);
#else
            if (Settings.debugEnabled)
                OnValueSet?.Invoke(_Value);
#endif
        }
    }
}