using Common.Ticker;
using UnityEngine.Events;
using AppodealAds.Unity.Api;
using ConsentManager;
using ConsentManager.Common;
using UnityEngine;

namespace Common.Managers.Advertising
{
    public interface IAppodealAdsProvider : IAdsProvider, IConsentInfoUpdateListener { }
    
    public class AppodealAdsProvider : AdsProviderCommonBase, IAppodealAdsProvider
    {
        #region nonpublic members

        protected override string AppId => Application.isEditor ? string.Empty : base.AppId;

        #endregion
        
        #region inject
        
        private IAppodealInterstitialAd InterstitialAd { get; }
        private IAppodealRewardedAd     RewardedAd     { get; }

        public AppodealAdsProvider(
            IAppodealInterstitialAd _InterstitialAd,
            IAppodealRewardedAd     _RewardedAd,
            IViewGameTicker         _ViewGameTicker) 
            : base(_InterstitialAd, _RewardedAd, _ViewGameTicker)
        {
            InterstitialAd = _InterstitialAd;
            RewardedAd     = _RewardedAd;
        }

        #endregion

        #region api

        public override string Source              => AdvertisingNetworks.Appodeal;
        public override bool   RewardedAdReady     => RewardedAd.Ready;
        public override bool   InterstitialAdReady => InterstitialAd.Ready;

        #endregion

        #region nonpublic methods

        protected override void InitConfigs(UnityAction _OnSuccess)
        {
            Appodeal.setTesting(TestMode);
            Appodeal.setLogLevel(Appodeal.LogLevel.Verbose);
            Appodeal.setChildDirectedTreatment(true);
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

        // User's consent status successfully updated.
        public void onConsentInfoUpdated(Consent _Consent)
        {
            Dbg.Log("onConsentInfoUpdated");
            // Initialize the Appodeal SDK with the received Consent object
            // here or show consent window
        }

        // User's consent status failed to update.
        public void onFailedToUpdateConsentInfo(ConsentManagerException _Error)
        {
            Dbg.Log($"onFailedToUpdateConsentInfo Reason: {_Error.getReason()}");
        }
    }
}