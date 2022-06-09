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
            Appodeal.setTesting(TestMode);
            Appodeal.setLogLevel(TestMode ? Appodeal.LogLevel.Debug : Appodeal.LogLevel.None);
            Appodeal.setChildDirectedTreatment(true);
            Appodeal.initialize(AppId, Appodeal.INTERSTITIAL | Appodeal.REWARDED_VIDEO);
            DisableUnusedNetworks();
        }

        protected override void InitRewardedAd()
        {
            RewardedAd.Init(AppId, RewardedUnitId);
        }

        protected override void InitInterstitialAd()
        {
            InterstitialAd.Init(AppId, InterstitialUnitId);
        }

        private static void DisableUnusedNetworks()
        {
            var unusedNetworks = new[]
            {
                AppodealNetworks.A4G,
                AppodealNetworks.NAST,
                AppodealNetworks.VAST,
                AppodealNetworks.FYBER,
                AppodealNetworks.MOPUB,
                AppodealNetworks.MRAID,
                AppodealNetworks.OGURY,
                AppodealNetworks.FLURRY,
                AppodealNetworks.INMOBI,
                AppodealNetworks.SMAATO,
                AppodealNetworks.TAPJOY,
                AppodealNetworks.VUNGLE,
                AppodealNetworks.ADCOLONY,
                AppodealNetworks.APPLOVIN,
                AppodealNetworks.APPODEAL,
                AppodealNetworks.FACEBOOK,
                AppodealNetworks.STARTAPP,
                AppodealNetworks.AMAZON_ADS,
                AppodealNetworks.MINTEGRAL,
                AppodealNetworks.CHARTBOOST,
                AppodealNetworks.IRONSOURCE,
                // AppodealNetworks.YANDEX,
                // AppodealNetworks.MY_TARGET,
                // AppodealNetworks.BIDMACHINE,
                // AppodealNetworks.ADMOB,
                // AppodealNetworks.UNITY_ADS,
            };
            foreach (string network in unusedNetworks)
                Appodeal.disableNetwork(network);
        }

        #endregion


    }
}