using GameHelpers;
using Managers.Advertising;
using Managers.Audio;
using Managers.IAP;
using Managers.Scores;

namespace Managers
{
    public interface IManagersGetter
    {
        IAudioManager        AudioManager        { get; }
        IAnalyticsManager    AnalyticsManager    { get; }
        IAdsManager          AdsManager          { get; } 
        IShopManager         ShopManager         { get; }
        ILocalizationManager LocalizationManager { get; }
        IScoreManager        ScoreManager        { get; }
        IHapticsManager      HapticsManager      { get; }
        IPrefabSetManager    PrefabSetManager    { get; }
        IAssetBundleManager  AssetBundleManager  { get; }
        IRemoteConfigManager RemoteConfigManager { get; }
    }

    public class ManagersGetter : IManagersGetter
    {
        #region inject
        
        public IAudioManager        AudioManager        { get; }
        public IAnalyticsManager    AnalyticsManager    { get; }
        public IAdsManager          AdsManager          { get; } 
        public IShopManager         ShopManager         { get; }
        public ILocalizationManager LocalizationManager { get; }
        public IScoreManager        ScoreManager        { get; }
        public IHapticsManager      HapticsManager      { get; }
        public IPrefabSetManager    PrefabSetManager    { get; }
        public IAssetBundleManager  AssetBundleManager  { get; }
        public IRemoteConfigManager RemoteConfigManager { get; }

        public ManagersGetter(
            IAudioManager        _AudioManager,
            IAnalyticsManager    _AnalyticsManager,
            IAdsManager          _AdsManager,
            IShopManager         _ShopManager,
            ILocalizationManager _LocalizationManager,
            IScoreManager        _ScoreManager,
            IHapticsManager      _HapticsManager,
            IPrefabSetManager    _PrefabSetManager,
            IAssetBundleManager  _AssetBundleManager,
            IRemoteConfigManager _RemoteConfigManager)
        {
            AudioManager        = _AudioManager;
            AnalyticsManager    = _AnalyticsManager;
            AdsManager          = _AdsManager;
            ShopManager         = _ShopManager;
            LocalizationManager = _LocalizationManager;
            ScoreManager        = _ScoreManager;
            HapticsManager      = _HapticsManager;
            PrefabSetManager    = _PrefabSetManager;
            AssetBundleManager  = _AssetBundleManager;
            RemoteConfigManager = _RemoteConfigManager;
        }
        
        #endregion

        #region api
        

        #endregion
    }
}