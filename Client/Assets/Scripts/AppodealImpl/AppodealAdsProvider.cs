#if APPODEAL
using AppodealAds.Unity.Api;
using Common;
using Common.Managers.Advertising;
using Common.Ticker;
using ConsentManager;
using ConsentManager.Common;
using UnityEngine;
using UnityEngine.Events;

namespace AppodealImpl
{
    public interface IAppodealAdsProvider : IAdsProvider, IConsentInfoUpdateListener { }
    
    public class AppodealAdsProvider : AdsProviderCommonBase, IAppodealAdsProvider
    {
        #region nonpublic members

        protected override string AppId => Application.isEditor ? string.Empty : base.AppId;

        private Consent     m_Consent;
        private UnityAction m_OnSuccess;

        #endregion
        
        #region inject
        
        private IAppodealInterstitialAd InterstitialAd { get; }
        private IAppodealRewardedAd     RewardedAd     { get; }

        private AppodealAdsProvider(
            IAppodealInterstitialAd _InterstitialAd,
            IAppodealRewardedAd     _RewardedAd,
            IViewGameTicker         _ViewGameTicker,
            IModelGameTicker        _ModelGameTicker) 
            : base(_InterstitialAd, _RewardedAd, _ViewGameTicker, _ModelGameTicker)
        {
            InterstitialAd = _InterstitialAd;
            RewardedAd     = _RewardedAd;
        }

        #endregion

        #region api

        public override string Source              => AdvertisingNetworks.Appodeal;
        public override bool   RewardedAdReady     => RewardedAd.Ready;
        public override bool   InterstitialAdReady => InterstitialAd.Ready;
        
        // User's consent status successfully updated.
        public void onConsentInfoUpdated(Consent _Consent)
        {
            Dbg.Log("onConsentInfoUpdated");
            // Initialize the Appodeal SDK with the received Consent object
            // here or show consent window
            m_Consent = _Consent;
            Appodeal.initialize(AppId, Appodeal.INTERSTITIAL | Appodeal.REWARDED_VIDEO, m_Consent);
            m_OnSuccess?.Invoke();
        }

        // User's consent status failed to update.
        public void onFailedToUpdateConsentInfo(ConsentManagerException _Error)
        {
            Dbg.Log($"onFailedToUpdateConsentInfo Reason: {_Error.getReason()}");
        }

        #endregion

        #region nonpublic methods

        protected override void InitConfigs(UnityAction _OnSuccess)
        {
            m_OnSuccess = _OnSuccess;
            Appodeal.setTesting(TestMode);
            Appodeal.setLogLevel(TestMode ? Appodeal.LogLevel.Verbose : Appodeal.LogLevel.None);
            Appodeal.setChildDirectedTreatment(true);
            Appodeal.setAutoCache(Appodeal.INTERSTITIAL | Appodeal.REWARDED_VIDEO, true);
            Appodeal.muteVideosIfCallsMuted(true);
            var consentManager = ConsentManager.ConsentManager.getInstance();
            consentManager.requestConsentInfoUpdate(AppId, this);
        }

        protected override void InitRewardedAd()
        {
            RewardedAd.Init(AppId, RewardedUnitId);
        }

        protected override void InitInterstitialAd()
        {
            InterstitialAd.Init(AppId, InterstitialUnitId);
        }
        
        #endregion
    }
}
#endif