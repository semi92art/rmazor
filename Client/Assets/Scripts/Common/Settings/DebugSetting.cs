using Common.Entities;
using Common.Helpers;
using Common.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Settings
{
    public interface IDebugSetting : ISetting<bool> { }
    
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DebugSetting : SettingBase<bool>, IDebugSetting
    {

        #region nonpublic members
        
        private IRemotePropertiesCommon RemoteProperties { get; }

        #endregion

        #region inject
        
        private DebugSetting(IRemotePropertiesCommon _RemoteProperties)
        {
            RemoteProperties = _RemoteProperties;
        }

        #endregion

        #region api
        
        public override SaveKey<bool>    Key      => SaveKeysCommon.DebugUtilsOn;
        public override string           TitleKey => "Debug";
        public override ESettingLocation Location => ESettingLocation.Main;
        public override ESettingType     Type     => ESettingType.OnOff;
        
        public override void Put(bool _Value)
        {
            SaveUtils.PutValue(Key, _Value);
            if (Application.isEditor && (CommonData.DevelopmentBuild || RemoteProperties.DebugEnabled))
                RaiseValueSetEvent(_Value);
        }
        
        #endregion
    }
}