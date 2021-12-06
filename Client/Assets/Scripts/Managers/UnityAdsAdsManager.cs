using System.Linq;
using DI.Extensions;
using UnityEngine.Events;
using UnityEngine.Advertisements;
using Utils;

namespace Managers
{
    public class UnityAdsAdsManager : AdsManagerBase, IUnityAdsInitializationListener
    {
        private IUnityAdsInterstitialAd  InterstitialAd  { get; }
        private IUnityAdsRewardedAd RewardedAd { get; }

        public UnityAdsAdsManager(
            IUnityAdsInterstitialAd _InterstitialAd,
            IUnityAdsRewardedAd _RewardedAd,
            IShopManager _ShopManager) : base(_ShopManager)
        {
            InterstitialAd = _InterstitialAd;
            RewardedAd = _RewardedAd;
        }

        public override bool RewardedAdReady     => RewardedAd.Ready;
        public override bool InterstitialAdReady => InterstitialAd.Ready;
        
        public override void ShowRewardedAd(UnityAction _OnShown)
        {
            RewardedAd.ShowAd(_OnShown);
        }

        public override void ShowInterstitialAd(UnityAction _OnShown)
        {
            InterstitialAd.ShowAd(_OnShown);
        }

        protected override void InitConfigs(UnityAction _OnSuccess)
        {
            bool testMode = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            testMode = true;
#endif
            string gameId = GetGameId();
            Advertisement.Initialize(gameId, testMode, false, this);
            _OnSuccess?.Invoke();
        }

        protected override void InitRewardedAd()
        {
            RewardedAd.Init(GetAdsNodeValue("unity_ads", "reward"));
        }

        protected override void InitInterstitialAd()
        {
            InterstitialAd.Init(GetAdsNodeValue("unity_ads", "interstitial"));
        }

        private string GetGameId()
        {
            string os = null;
#if UNITY_ANDROID
            os = "android";
#elif UNITY_IOS || UNITY_IPHONE
            os = "ios";
#endif
            return m_Ads.Elements("game_id")
                .FirstOrDefault(_El => _El.Attribute("os")?.Value == os)
                ?.Value;
        }
        
        public void OnInitializationComplete()
        {
            Dbg.Log(nameof(OnInitializationComplete).WithSpaces());
        }

        public void OnInitializationFailed(UnityAdsInitializationError _Error, string _Message)
        {
            Dbg.Log(nameof(OnInitializationFailed).WithSpaces() + ": " + _Error + ": " + _Message);
        }
    }
}