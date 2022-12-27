using System;
using Common.Managers;
using Common.Managers.Advertising;
using Common.Managers.PlatformGameServices;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Managers.IAP;
using mazing.common.Runtime.Managers.Notifications;
using RMAZOR.Models;
using RMAZOR.Views;

namespace RMAZOR.Managers
{
    public interface IManagersGetter : IOnLevelStageChanged, IInit
    {
        IAudioManager         AudioManager         { get; }
        IAnalyticsManager     AnalyticsManager     { get; }
        IAdsManager           AdsManager           { get; } 
        IShopManager          ShopManager          { get; }
        ILocalizationManager  LocalizationManager  { get; }
        IScoreManager         ScoreManager         { get; }
        IHapticsManager       HapticsManager       { get; }
        IPrefabSetManager     PrefabSetManager     { get; }
        IRemoteConfigManager  RemoteConfigManager  { get; }
        IDebugManager         DebugManager         { get; }
        INotificationsManager NotificationsManager { get; }
    }

    public class ManagersGetter : InitBase, IManagersGetter
    {
        #region nonpublic members

        private object[] m_ProceedersCached;

        #endregion
        
        #region inject
        
        public IAudioManager         AudioManager         { get; }
        public IAnalyticsManager     AnalyticsManager     { get; }
        public IAdsManager           AdsManager           { get; } 
        public IShopManager          ShopManager          { get; }
        public ILocalizationManager  LocalizationManager  { get; }
        public IScoreManager         ScoreManager         { get; }
        public IHapticsManager       HapticsManager       { get; }
        public IPrefabSetManager     PrefabSetManager     { get; }
        public IRemoteConfigManager  RemoteConfigManager  { get; }
        public IDebugManager         DebugManager         { get; }
        public INotificationsManager NotificationsManager { get; }

        public ManagersGetter(
            IAudioManager         _AudioManager,
            IAnalyticsManager     _AnalyticsManager,
            IAdsManager           _AdsManager,
            IShopManager          _ShopManager,
            ILocalizationManager  _LocalizationManager,
            IScoreManager         _ScoreManager,
            IHapticsManager       _HapticsManager,
            IPrefabSetManager     _PrefabSetManager,
            IRemoteConfigManager  _RemoteConfigManager,
            IDebugManager         _DebugManager,
            INotificationsManager _NotificationsManager)
        {
            AudioManager         = _AudioManager;
            AnalyticsManager     = _AnalyticsManager;
            AdsManager           = _AdsManager;
            ShopManager          = _ShopManager;
            LocalizationManager  = _LocalizationManager;
            ScoreManager         = _ScoreManager;
            HapticsManager       = _HapticsManager;
            PrefabSetManager     = _PrefabSetManager;
            RemoteConfigManager  = _RemoteConfigManager;
            DebugManager         = _DebugManager;
            NotificationsManager = _NotificationsManager;
        }
        
        #endregion

        #region api

        public override void Init()
        {
            ShopManager.ShowAlertDialogAction = MazorCommonUtils.ShowAlertDialog;
            m_ProceedersCached = new object[]
            {
                AudioManager,
                AnalyticsManager,
                AdsManager,
                ShopManager,
                LocalizationManager,
                ScoreManager,
                HapticsManager,
                PrefabSetManager,
                RemoteConfigManager,
                NotificationsManager
            };
            var proceeders = GetInterfaceOfProceeders<IInit>();
            foreach (var proceeder in proceeders)
                proceeder?.Init();
            base.Init();
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<IOnLevelStageChanged>();
            foreach (var proceeder in proceeders)
                proceeder?.OnLevelStageChanged(_Args);
        }
        
        #endregion

        #region nonpublic methods

        private T[] GetInterfaceOfProceeders<T>() where T : class
        {
            return Array.ConvertAll(m_ProceedersCached, _Item => _Item as T);
        }

        #endregion
    }
}