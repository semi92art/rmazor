using System.Linq;
using Common.Entities;
using Common.Utils;
using UnityEngine.Advertisements;
using UnityEngine.Events;

namespace Common.Managers.Advertising
{
    public class UnityAdsProvider : AdsProviderBase, IUnityAdsInitializationListener
    {
        private IUnityAdsInterstitialAd InterstitialAd { get; }
        private IUnityAdsRewardedAd     RewardedAd     { get; }

        public UnityAdsProvider(
            IUnityAdsInterstitialAd _InterstitialAd,
            IUnityAdsRewardedAd     _RewardedAd,
            bool                    _TestMode,
            float                   _ShowRate) 
            : base(_TestMode, _ShowRate)
        {
            InterstitialAd = _InterstitialAd;
            RewardedAd = _RewardedAd;
        }

        public override EAdsProvider Provider => EAdsProvider.UnityAds;

        public override bool RewardedAdReady
        {
            get
            {
#if UNITY_EDITOR
                return true;
#endif
                return RewardedAd != null && RewardedAd.Ready;
            }
        }

        public override bool InterstitialAdReady
        {
            get
            {
#if UNITY_EDITOR
                return true;
#endif
                return InterstitialAd != null && InterstitialAd.Ready;
            }
        }

        public override void ShowRewardedAd(UnityAction _OnShown, BoolEntity _ShowAds)
        {
            Dbg.Log("Start showing UnityAds Rewarded Ad");
            Cor.Run(Cor.WaitWhile(
                () => _ShowAds.Result == EEntityResult.Pending,
                () =>
                {
                    if (_ShowAds.Result != EEntityResult.Success)
                        return;
                    if (!_ShowAds.Value)
                        return;
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
                }));
        }

        public override void ShowInterstitialAd(UnityAction _OnShown, BoolEntity _ShowAds)
        {
            Dbg.Log("Start showing UnityAds Interstitial Ad");
            Cor.Run(Cor.WaitWhile(
                () => _ShowAds.Result == EEntityResult.Pending,
                () =>
                {
                    if (_ShowAds.Result != EEntityResult.Success)
                        return;
                    if (!_ShowAds.Value)
                        return;
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
                }));
        }

        protected override void InitConfigs(UnityAction _OnSuccess)
        {
            string gameId = GetGameId();
            Advertisement.Initialize(gameId, TestMode, this);
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
            return AdsData.Elements("game_id")
                .FirstOrDefault(_El => _El.Attribute("os")?.Value == os)
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