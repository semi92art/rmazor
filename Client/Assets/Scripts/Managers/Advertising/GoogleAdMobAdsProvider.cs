using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Utils;
using GameHelpers;
using GoogleMobileAds.Api;
using Managers.IAP;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Managers.Advertising
{
    public class GoogleAdMobAdsProvider : AdsProviderBase
    {

        #region nonpublic members

        private          RewardedAd     m_RewardedAd;
        private          InterstitialAd m_InterstitialAd;

        #endregion

        #region inject
        private CommonGameSettings Settings { get; }

        public GoogleAdMobAdsProvider(
            CommonGameSettings _Settings, 
            IShopManager _ShopManager,
            bool _TestMode)
            : base(_ShopManager, _TestMode)
        {
            Settings = _Settings;
        }

        #endregion

        #region api
        
        public override bool RewardedAdReady     => m_RewardedAd != null && m_RewardedAd.IsLoaded();
        public override bool InterstitialAdReady => m_InterstitialAd != null && m_InterstitialAd.IsLoaded();
        
        public override void ShowRewardedAd(UnityAction _OnShown, BoolEntity _ShowAds)
        {
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
                        m_OnRewardedAdShown = _OnShown;
                        m_RewardedAd.Show();
                    }
                    else
                    {
                        Dbg.Log("Rewarded ad is not ready.");
                        var adRequest = new AdRequest.Builder().Build();
                        m_RewardedAd.LoadAd(adRequest);
                    }
                }));
        }
        
        public override void ShowInterstitialAd(UnityAction _OnShown, BoolEntity _ShowAds)
        {
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
                        m_OnInterstitialShown = _OnShown;
                        m_InterstitialAd.Show();
                    }
                    else
                    {
                        Dbg.Log("Interstitial ad is not ready.");
                        var adRequest = new AdRequest.Builder().Build();
                        m_InterstitialAd.LoadAd(adRequest);
                    }
                }));
        }
        
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
            string testId = Application.platform == RuntimePlatform.Android
                ? "ca-app-pub-3940256099942544/5224354917"  // https://developers.google.com/admob/android/test-ads
                : "ca-app-pub-3940256099942544/1712485313"; // https://developers.google.com/admob/ios/test-ads
            string rewardedAdId = m_TestMode ? testId : GetAdsNodeValue("admob", "reward");
            m_RewardedAd = new RewardedAd(rewardedAdId);
            var adRequest = new AdRequest.Builder().Build();
            m_RewardedAd.LoadAd(adRequest);
            m_RewardedAd.OnAdLoaded              += OnRewardedAdLoaded;
            m_RewardedAd.OnAdFailedToLoad        += OnRewardedAdFailedToLoad;
            m_RewardedAd.OnPaidEvent             += OnRewardedAdPaidEvent;
            m_RewardedAd.OnAdClosed              += OnRewardedAdClosed;
            m_RewardedAd.OnUserEarnedReward      += OnRewardedAdUserEarnedReward;
            m_RewardedAd.OnAdDidRecordImpression += OnRewardedAdDidRecordImpression;
        }

        protected override void InitInterstitialAd()
        {
            string testId = Application.platform == RuntimePlatform.Android
                ? "ca-app-pub-3940256099942544/8691691433"  // https://developers.google.com/admob/android/test-ads
                : "ca-app-pub-3940256099942544/5135589807"; // https://developers.google.com/admob/ios/test-ads
            string interstitialAdId = m_TestMode ? testId : GetAdsNodeValue("admob", "interstitial");
            m_InterstitialAd = new InterstitialAd(interstitialAdId);
            var adRequest = new AdRequest.Builder().Build();
            m_InterstitialAd.LoadAd(adRequest);
            m_InterstitialAd.OnAdLoaded       += OnInterstitialAdLoaded;
            m_InterstitialAd.OnAdFailedToLoad += OnInterstitialAdFailedToLoad;
            m_InterstitialAd.OnAdClosed       += OnInterstitialAdClosed;
        }
        
        private List<string> GetTestDeviceIds()
        {
            return m_AdsData.Elements("test_device").Select(_El => _El.Value).ToList();
        }

        #endregion

        #region event methods

        private void OnRewardedAdLoaded(object _Sender, EventArgs _E)
        {
            Dbg.Log("GoogleAds: " + nameof(OnRewardedAdLoaded));
        }
        
        private void OnRewardedAdFailedToLoad(object _Sender, AdFailedToLoadEventArgs _E)
        {
            Dbg.Log("GoogleAds: " + nameof(OnRewardedAdFailedToLoad));
        }
        
        private void OnRewardedAdPaidEvent(object _Sender, AdValueEventArgs _E)
        {
            Dbg.Log("GoogleAds: " + nameof(OnRewardedAdPaidEvent));
        }
        
        private void OnRewardedAdClosed(object _Sender, EventArgs _E)
        {
            Dbg.Log("GoogleAds: " + nameof(OnRewardedAdClosed));
            m_OnRewardedAdShown?.Invoke();
            var adRequest = new AdRequest.Builder().Build();
            m_RewardedAd.LoadAd(adRequest);
        }
        
        private void OnRewardedAdDidRecordImpression(object _Sender, EventArgs _E)
        {
            Dbg.Log(nameof(OnRewardedAdDidRecordImpression));
        }

        private void OnRewardedAdUserEarnedReward(object _Sender, Reward _E)
        {
            Dbg.Log("GoogleAds: " + nameof(OnRewardedAdUserEarnedReward)
                                  + ": amount: " + _E.Amount + 
                                  ", type: " + _E.Type);
        }
        
        private void OnInterstitialAdLoaded(object _Sender, EventArgs _E)
        {
            Dbg.Log("GoogleAds: " + nameof(OnInterstitialAdLoaded));
        }
        
        private void OnInterstitialAdFailedToLoad(object _Sender, AdFailedToLoadEventArgs _E)
        {
            Dbg.Log("GoogleAds: " + nameof(OnInterstitialAdFailedToLoad));
        }
        
        private void OnInterstitialAdClosed(object _Sender, EventArgs _E)
        {
            Dbg.Log("GoogleAds: " + nameof(OnInterstitialAdClosed));
            var adRequest = new AdRequest.Builder().Build();
            m_InterstitialAd.LoadAd(adRequest);
        }
        
        #endregion

    }
}