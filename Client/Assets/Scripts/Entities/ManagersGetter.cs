using Controllers;
using Managers;
using Zenject;

namespace Entities
{
    public delegate void SoundManagerHandler(ISoundManager Manager);
    public delegate void AdsManagerHandler(IAdsManager Manager);
    public delegate void AnalyticsManagerHandler(IAnalyticsManager Manager);
    public delegate void ShopManagerHandler(IShopManager Manager);
    public delegate void LocalizationManagerHandler(ILocalizationManager Manager);
    public delegate void ScoreManagerHandler(IScoreManager Manager);
    
    public interface IManagersGetter
    {
        ISoundManager        SoundManager { get; }
        IAnalyticsManager    AnalyticsManager { get; }
        IAdsManager          AdsManager { get; } 
        IShopManager         ShopManager { get; }
        ILocalizationManager LocalizationManager { get; }
        IScoreManager        ScoreManager { get; }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        IDebugManager        DebugManager { get; }
#endif
        void Notify(
            SoundManagerHandler _OnSoundManager = null,
            AnalyticsManagerHandler _OnAnalyticsManager = null,
            AdsManagerHandler _OnAdsManager = null,
            ShopManagerHandler _OnShopManager = null,
            LocalizationManagerHandler _LocalizationManager = null,
            ScoreManagerHandler _ScoreManager = null);
    }

    public class ManagersGetter : IManagersGetter
    {
        #region inject
        
        public ISoundManager        SoundManager { get; }
        public IAnalyticsManager    AnalyticsManager { get; }
        public IAdsManager          AdsManager { get; } 
        public IShopManager         ShopManager { get; }
        public ILocalizationManager LocalizationManager { get; }
        public IScoreManager        ScoreManager { get; }

        public ManagersGetter(
            ISoundManager        _SoundManager,
            IAnalyticsManager    _AnalyticsManager,
            IAdsManager          _AdsManager,
            IShopManager         _ShopManager,
            ILocalizationManager _LocalizationManager,
            IScoreManager        _ScoreManager)
        {
            SoundManager        = _SoundManager;
            AnalyticsManager    = _AnalyticsManager;
            AdsManager          = _AdsManager;
            ShopManager         = _ShopManager;
            LocalizationManager = _LocalizationManager;
            ScoreManager        = _ScoreManager;
        }
        
        #endregion

        #region api
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD

        [Inject] public IDebugManager DebugManager { get; }
#endif

        public void Notify(
            SoundManagerHandler           _OnSoundManager = null,
            AnalyticsManagerHandler       _OnAnalyticsManager = null,
            AdsManagerHandler             _OnAdsManager = null,
            ShopManagerHandler       _OnShopManager = null,
            LocalizationManagerHandler    _LocalizationManager = null,
            ScoreManagerHandler           _ScoreManager = null)
        {
            _OnSoundManager         ?.Invoke(SoundManager);
            _OnAnalyticsManager     ?.Invoke(AnalyticsManager);
            _OnAdsManager           ?.Invoke(AdsManager);
            _OnShopManager     ?.Invoke(ShopManager);
            _LocalizationManager    ?.Invoke(LocalizationManager);
            _ScoreManager           ?.Invoke(ScoreManager);
        }

        #endregion
    }
}