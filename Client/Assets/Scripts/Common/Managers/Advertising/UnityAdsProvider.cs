#if UNITY_ADS_API

using Common.Ticker;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Advertisements;

namespace Common.Managers.Advertising
{
    public interface IUnityAdsProvider : IAdsProvider { }
    
    public class UnityAdsProvider : AdsProviderCommonBase, IUnityAdsProvider, IUnityAdsInitializationListener
    {
        private IUnityAdsInterstitialAd InterstitialAd { get; }
        private IUnityAdsRewardedAd     RewardedAd     { get; }
    
        public UnityAdsProvider(
            IUnityAdsInterstitialAd _InterstitialAd,
            IUnityAdsRewardedAd     _RewardedAd) 
            : base(_InterstitialAd, _RewardedAd)
        {
            InterstitialAd = _InterstitialAd;
            RewardedAd     = _RewardedAd;
        }
        
        public override string Source => AdvertisingNetworks.UnityAds;
        
        public override bool RewardedAdReady => 
            Application.isEditor || (RewardedAd != null && RewardedAd.Ready);
    
        public override bool InterstitialAdReady => 
            Application.isEditor || (InterstitialAd != null && InterstitialAd.Ready);
    
        protected override void InitConfigs(UnityAction _OnSuccess)
        {
            SetMetadata();
            Advertisement.Initialize(AppId, TestMode, this);
            _OnSuccess?.Invoke();
        }
    
        private static void SetMetadata()
        {
            // Starting April 1, 2022, the Google Families Policy will no longer allow Device IDs
            // to leave users’ devices from apps where one of the target audiences is children
            // https://docs.unity.com/ads/GoogleFamiliesCompliance.html
            MetaData metaData = new MetaData("privacy");
            metaData.Set("mode", "mixed"); // This is a mixed audience game.
            Advertisement.SetMetaData(metaData);
            metaData = new MetaData("user");
            metaData.Set("nonbehavioral", "true"); // This user will NOT receive personalized ads.
            Advertisement.SetMetaData(metaData);
        }
    
        protected override void InitRewardedAd()
        {
            RewardedAd.Init(AppId, RewardedUnitId);
        }
    
        protected override void InitInterstitialAd()
        {
            InterstitialAd.Init(AppId, InterstitialUnitId);
        }

        public void OnInitializationComplete()
        {
            Dbg.Log($"{nameof(UnityAdsProvider)} {nameof(OnInitializationComplete)}");
        }
    
        public void OnInitializationFailed(UnityAdsInitializationError _Error, string _Message)
        {
            Dbg.Log($"{nameof(UnityAdsProvider)} " +
                    $"{nameof(OnInitializationFailed)}" + ": " + _Error + ": " + _Message);
        }
    }
}

#endif