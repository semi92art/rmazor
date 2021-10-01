using Controllers;
using DebugConsole;
using Managers;

namespace Entities
{
    public delegate void SoundManagerHandler(ISoundManager Manager);
    public delegate void AdsManagerHandler(IAdsManager Manager);
    public delegate void AnalyticsManagerHandler(IAnalyticsManager Manager);
    public delegate void PurchasesManagerHandler(IPurchasesManager Manager);
    
    public interface IManagersGetter
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        IDebugConsoleController DebugConsole { get; }
#endif
        void Notify(
            SoundManagerHandler     _OnSoundManager = null,
            AnalyticsManagerHandler _OnAnalyticsManager = null,
            AdsManagerHandler       _OnAdsManager = null,
            PurchasesManagerHandler _OnPurchasesManager = null);
    }

    public class ManagersGetter : IManagersGetter
    {
        #region inject
        
        private ISoundManager     SoundManager { get; }
        private IAnalyticsManager AnalyticsManager { get; }
        private IAdsManager       AdsManager { get; } 
        private IPurchasesManager PurchasesManager { get; }

        public ManagersGetter(
            ISoundManager     _SoundManager,
            IAnalyticsManager _AnalyticsManager,
            IAdsManager       _AdsManager,
            IPurchasesManager _PurchasesManager)
        {
            SoundManager      = _SoundManager;
            AnalyticsManager  = _AnalyticsManager;
            AdsManager        = _AdsManager;
            PurchasesManager  = _PurchasesManager;
        }
        
        #endregion

        #region api
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD

        public IDebugConsoleController DebugConsole { get; } = DebugConsoleView.Instance.Controller;
        
#endif

        public void Notify(
            SoundManagerHandler     _OnSoundManager = null,
            AnalyticsManagerHandler _OnAnalyticsManager = null,
            AdsManagerHandler       _OnAdsManager = null,
            PurchasesManagerHandler _OnPurchasesManager = null)
        {
            _OnSoundManager         ?.Invoke(SoundManager);
            _OnAnalyticsManager     ?.Invoke(AnalyticsManager);
            _OnAdsManager           ?.Invoke(AdsManager);
            _OnPurchasesManager     ?.Invoke(PurchasesManager);
        }

        #endregion
    }
}