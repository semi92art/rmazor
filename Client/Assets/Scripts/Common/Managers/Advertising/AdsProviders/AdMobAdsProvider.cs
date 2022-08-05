#if ADMOB_API

using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using Common.Utils;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Events;
using Common.Managers.Advertising.AdBlocks;

namespace Common.Managers.Advertising.AdsProviders
{
    public interface IAdMobAdsProvider : IAdsProvider { }
    
    public class AdMobAdsProvider : AdsProviderCommonBase, IAdMobAdsProvider
    {
        #region nonpublic members

        protected override string InterstitialUnitId
        {
            get
            {
                string testId = CommonUtils.Platform == RuntimePlatform.Android
                    ? "ca-app-pub-3940256099942544/5224354917"  // https://developers.google.com/admob/android/test-ads
                    : "ca-app-pub-3940256099942544/1712485313"; // https://developers.google.com/admob/ios/test-ads
                return TestMode ? testId : base.InterstitialUnitId;
            }
        }

        protected override string RewardedUnitId
        {
            get
            {
                string testId = CommonUtils.Platform == RuntimePlatform.Android
                    ? "ca-app-pub-3940256099942544/8691691433"  // https://developers.google.com/admob/android/test-ads
                    : "ca-app-pub-3940256099942544/5135589807"; // https://developers.google.com/admob/ios/test-ads
                return TestMode ? testId : base.RewardedUnitId;
            }
        }

        #endregion
        
        #region inject

        private IRemotePropertiesCommon RemoteProperties { get; }

        private AdMobAdsProvider(
            IRemotePropertiesCommon _RemoteProperties,
            IAdMobInterstitialAd    _InterstitialAd,
            IAdMobRewardedAd        _RewardedAd,
            IAdMobRewardedAd        _RewardedAdNonSkippable) 
            : base(
                _InterstitialAd,
                _RewardedAd,
                _RewardedAdNonSkippable)
        {
            RemoteProperties = _RemoteProperties;
        }

        #endregion

        #region api

        public override string Source => AdvertisingNetworks.Admob;

        #endregion

        #region nonpublic methods

        protected override void InitConfigs(UnityAction _OnSuccess)
        {
            var reqConfig = new RequestConfiguration.Builder()
                .SetTestDeviceIds(GetTestDeviceIds())
                .build();
            MobileAds.SetiOSAppPauseOnBackground(true);
            MobileAds.DisableMediationInitialization();
            MobileAds.SetRequestConfiguration(reqConfig);
            MobileAds.Initialize(_InitStatus =>
            {
                var map = _InitStatus.getAdapterStatusMap();
                foreach ((string key, var value) in map)
                {
                    Dbg.Log($"Google AdMob initialization status: {key}:" +
                            $"{value.Description}:{value.InitializationState}");
                }
                _OnSuccess?.Invoke();
            });
        }

        private List<string> GetTestDeviceIds()
        {
            return RemoteProperties.TestDeviceIdsForAdmob ??
                   AdsData.Elements("test_device").Select(_El => _El.Value).ToList();
        }

        #endregion
    }
}

#endif