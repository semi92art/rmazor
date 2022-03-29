using System.Collections.Generic;
using System.Linq;
using Common.Ticker;
using Common.Utils;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Managers.Advertising
{
    public interface IAdMobAdsProvider : IAdsProvider { }
    
    public class AdMobAdsProvider : AdsProviderCommonBase, IAdMobAdsProvider
    {
        #region inject
        
        private IAdMobInterstitialAd InterstitialAd { get; }
        private IAdMobRewardedAd     RewardedAd     { get; }

        public AdMobAdsProvider(
            IAdMobInterstitialAd _InterstitialAd,
            IAdMobRewardedAd     _RewardedAd,
            IViewGameTicker      _ViewGameTicker) 
            : base(_InterstitialAd, _RewardedAd, _ViewGameTicker)
        {
            InterstitialAd = _InterstitialAd;
            RewardedAd     = _RewardedAd;
        }

        #endregion

        #region api

        public override EAdsProvider Provider            => EAdsProvider.AdMob;
        public override bool         RewardedAdReady     => RewardedAd.Ready;
        public override bool         InterstitialAdReady => InterstitialAd.Ready;

        #endregion

        #region nonpublic methods

        protected override void InitConfigs(UnityAction _OnSuccess)
        {
            var reqConfig = new RequestConfiguration.Builder()
                .SetTestDeviceIds(GetTestDeviceIds())
                .build();
            MobileAds.SetRequestConfiguration(reqConfig);
            MobileAds.Initialize(_InitStatus =>
            {
                var map = _InitStatus.getAdapterStatusMap();
                foreach (var kvp in map)
                {
                    Dbg.Log($"Google AdMob initialization status: {kvp.Key}:" +
                            $"{kvp.Value.Description}:{kvp.Value.InitializationState}");
                }
                _OnSuccess?.Invoke();
            });
        }

        protected override void InitRewardedAd()
        {
            string testId = CommonUtils.Platform == RuntimePlatform.Android
                ? "ca-app-pub-3940256099942544/5224354917"  // https://developers.google.com/admob/android/test-ads
                : "ca-app-pub-3940256099942544/1712485313"; // https://developers.google.com/admob/ios/test-ads
            string unitId = TestMode ? testId : GetAdsNodeValue("admob", "reward");
            RewardedAd.Init(unitId);
        }

        protected override void InitInterstitialAd()
        {
            string testId = CommonUtils.Platform == RuntimePlatform.Android
                ? "ca-app-pub-3940256099942544/8691691433"  // https://developers.google.com/admob/android/test-ads
                : "ca-app-pub-3940256099942544/5135589807"; // https://developers.google.com/admob/ios/test-ads
            string unitId = TestMode ? testId : GetAdsNodeValue("admob", "interstitial");
            InterstitialAd.Init(unitId);
        }

        private List<string> GetTestDeviceIds()
        {
            return AdsData.Elements("test_device").Select(_El => _El.Value).ToList();
        }

        #endregion
    }
}