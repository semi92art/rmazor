using System.Linq;
using Common.Ticker;
using Common.Utils;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Events;

namespace Common.Managers.Advertising
{
    public interface IUnityAdsProvider : IAdsProvider { }
    
    public class UnityAdsProvider : AdsProviderCommonBase, IUnityAdsProvider, IUnityAdsInitializationListener
    {
        private IUnityAdsInterstitialAd InterstitialAd { get; }
        private IUnityAdsRewardedAd     RewardedAd     { get; }

        public UnityAdsProvider(
            IUnityAdsInterstitialAd _InterstitialAd,
            IUnityAdsRewardedAd     _RewardedAd,
            IViewGameTicker         _ViewGameTicker) 
            : base(_InterstitialAd, _RewardedAd, _ViewGameTicker)
        {
            InterstitialAd = _InterstitialAd;
            RewardedAd     = _RewardedAd;
        }

        public override EAdsProvider Provider => EAdsProvider.UnityAds;

        public override bool RewardedAdReady
        {
            get
            {
                if (Application.isEditor)
                    return true;
                return RewardedAd != null && RewardedAd.Ready;
            }
        }

        public override bool InterstitialAdReady
        {
            get
            {
                if (Application.isEditor)
                    return true;
                return InterstitialAd != null && InterstitialAd.Ready;
            }
        }

        protected override void InitConfigs(UnityAction _OnSuccess)
        {
            SetMetadata();
            string gameId = GetGameId();
            Advertisement.Initialize(gameId, TestMode, this);
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
            RewardedAd.Init(GetAdsNodeValue("unity_ads", "reward"));
        }

        protected override void InitInterstitialAd()
        {
            InterstitialAd.Init(GetAdsNodeValue("unity_ads", "interstitial"));
        }

        protected override void ShowRewardedAdCore(UnityAction _OnShown)
        {
            if (RewardedAdReady)
            {
                OnRewardedAdShown = _OnShown;
                RewardedAd.ShowAd(_OnShown);
            }
            else
            {
                Dbg.Log("Rewarded ad is not ready.");
                RewardedAd.LoadAd();
            }
        }

        protected override void ShowInterstitialAdCore(UnityAction _OnShown)
        {
            if (InterstitialAdReady)
            {
                OnInterstitialAdShown = _OnShown;
                InterstitialAd.ShowAd(_OnShown);
            }
            else
            {
                Dbg.Log("Interstitial ad is not ready.");
                InterstitialAd.LoadAd();
            }
        }

        private string GetGameId()
        {
            return AdsData.Elements("game_id")
                .FirstOrDefault(_El => _El.Attribute("os")?.Value == CommonUtils.GetOsName().ToLower())
                ?.Value;
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