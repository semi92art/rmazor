﻿using System.Linq;
using Entities;
using UnityEngine.Advertisements;
using UnityEngine.Events;
using Utils;

namespace Managers.Advertising
{
    public class UnityAdsProvider : AdsProviderBase, IUnityAdsInitializationListener
    {
        private bool m_TestMode;
        
        private IUnityAdsInterstitialAd  InterstitialAd  { get; }
        private IUnityAdsRewardedAd RewardedAd { get; }

        public UnityAdsProvider(
            IUnityAdsInterstitialAd _InterstitialAd,
            IUnityAdsRewardedAd _RewardedAd,
            IShopManager _ShopManager) : base(_ShopManager)
        {
            InterstitialAd = _InterstitialAd;
            RewardedAd = _RewardedAd;
        }

        public override bool RewardedAdReady => m_TestMode || RewardedAd.Ready;

        public override bool InterstitialAdReady => m_TestMode || InterstitialAd.Ready;

        public override void ShowRewardedAd(UnityAction _OnShown, BoolEntity _ShowAds)
        {
            Coroutines.Run(Coroutines.WaitWhile(
                () => _ShowAds.Result == EEntityResult.Pending,
                () =>
                {
                    if (_ShowAds.Result != EEntityResult.Success)
                        return;
                    if (!_ShowAds.Value)
                        return;
                    if (RewardedAdReady)
                    {
                        m_OnRewardedAdShown = _OnShown;
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
            Coroutines.Run(Coroutines.WaitWhile(
                () => _ShowAds.Result == EEntityResult.Pending,
                () =>
                {
                    if (_ShowAds.Result != EEntityResult.Success)
                        return;
                    if (!_ShowAds.Value)
                        return;
                    if (InterstitialAdReady)
                    {
                        m_OnInterstitialShown = _OnShown;
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
            m_TestMode = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            m_TestMode = true;
#endif
            string gameId = GetGameId();
            Advertisement.Initialize(gameId, m_TestMode, this);
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
            return m_AdsData.Elements("game_id")
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