using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Settings;
using RMAZOR.Constants;
using UnityEngine.Events;

namespace RMAZOR.Settings
{
    public interface IRetroModeSetting : ISetting<bool>
    {
        event UnityAction StateUpdated;
        void              UpdateState();
    }
    
    public class RetroModeSetting : SettingBase<bool>, IRetroModeSetting
    {
        private IAnalyticsManager AnalyticsManager { get; }

        private RetroModeSetting(IAnalyticsManager _AnalyticsManager)
        {
            AnalyticsManager = _AnalyticsManager;
        }
        
        public event UnityAction StateUpdated;
        
        public override SaveKey<bool>    Key      => SaveKeysRmazor.RetroModeOn;
        public override string           TitleKey => "retro_mode";
        public override ESettingLocation Location => ESettingLocation.Main;
        public override ESettingType     Type     => ESettingType.OnOff;
        
        public override void Put(bool _Value)
        {
            AnalyticsManager.SendAnalytic(_Value 
                ? AnalyticIdsRmazor.RetroModeOnButtonPressed 
                : AnalyticIdsRmazor.RetroModeOffButtonPressed);
            base.Put(_Value);
        }

        public void UpdateState()
        {
            StateUpdated?.Invoke();
        }
    }
}