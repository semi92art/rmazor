using System;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Entities;
using GameHelpers;
using GoogleMobileAds.Api;
using UnityEngine.Events;
using Utils;

namespace Managers.Advertising
{
    public class GoogleAdMobAdsProvider : AdsProviderBase
    {

        #region nonpublic members

        private RewardedAd     m_RewardedAd;
        private InterstitialAd m_InterstitialAd;
        private bool           m_ShowAds;

        #endregion

        #region inject
        private CommonGameSettings Settings { get; }

        public GoogleAdMobAdsProvider(CommonGameSettings _Settings, IShopManager _ShopManager, bool _TestMode)
            : base(_ShopManager, _TestMode)
        {
            Settings = _Settings;
        }

        #endregion

        #region api
        
        public override bool RewardedAdReady     => m_RewardedAd.IsLoaded();
        public override bool InterstitialAdReady => m_InterstitialAd.IsLoaded();
        
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
                    Dbg.Log($"Google AdMob initialization status: {kvp.Key}:{kvp.Value.Description}:{kvp.Value.InitializationState}");
                }
                _OnSuccess?.Invoke();
            });
        }

        protected override void InitRewardedAd()
        {
            string rewardedAdId = GetAdsNodeValue("admob", "interstitial");
            m_RewardedAd = new RewardedAd(rewardedAdId);
            var adRequest = new AdRequest.Builder().Build();
            m_RewardedAd.LoadAd(adRequest);
            m_RewardedAd.OnAdLoaded += OnRewardedAdLoaded;
            m_RewardedAd.OnAdFailedToLoad += OnRewardedAdFailedToLoad;
            m_RewardedAd.OnPaidEvent += OnRewardedAdPaidEvent;
            m_RewardedAd.OnAdClosed += OnRewardedAdClosed;
        }
        
        protected override void InitInterstitialAd()
        {
            string interstitialAdId = GetAdsNodeValue("admob", "reward");
            m_InterstitialAd = new InterstitialAd(interstitialAdId);
            var adRequest = new AdRequest.Builder().Build();
            m_InterstitialAd.LoadAd(adRequest);
            m_InterstitialAd.OnAdLoaded += OnInterstitialAdLoaded;
            m_InterstitialAd.OnAdFailedToLoad += OnInterstitialAdFailedToLoad;
            m_InterstitialAd.OnAdClosed += OnInterstitialAdClosed;
        }
        
        private List<string> GetTestDeviceIds()
        {
            return m_AdsData.Elements("test_device").Select(_El => _El.Value).ToList();
        }

        #endregion

        #region event methods

        private void OnRewardedAdLoaded(object _Sender, EventArgs _E)
        {
            Dbg.Log(nameof(OnRewardedAdLoaded).WithSpaces());
        }
        
        private void OnRewardedAdFailedToLoad(object _Sender, AdFailedToLoadEventArgs _E)
        {
            Dbg.Log(nameof(OnRewardedAdFailedToLoad).WithSpaces());
        }
        
        private void OnRewardedAdPaidEvent(object _Sender, AdValueEventArgs _E)
        {
            Dbg.Log(nameof(OnRewardedAdPaidEvent).WithSpaces());
        }
        
        private void OnRewardedAdClosed(object _Sender, EventArgs _E)
        {
            Dbg.Log(nameof(OnRewardedAdClosed).WithSpaces());
            m_OnRewardedAdShown?.Invoke();
            var adRequest = new AdRequest.Builder().Build();
            m_RewardedAd.LoadAd(adRequest);
        }
        
        private void OnInterstitialAdLoaded(object _Sender, EventArgs _E)
        {
            Dbg.Log(nameof(OnInterstitialAdLoaded).WithSpaces());
        }
        
        private void OnInterstitialAdFailedToLoad(object _Sender, AdFailedToLoadEventArgs _E)
        {
            Dbg.Log(nameof(OnInterstitialAdFailedToLoad).WithSpaces());
        }
        
        private void OnInterstitialAdClosed(object _Sender, EventArgs _E)
        {
            Dbg.Log(nameof(OnInterstitialAdClosed).WithSpaces());
            var adRequest = new AdRequest.Builder().Build();
            m_InterstitialAd.LoadAd(adRequest);
        }
        
        #endregion

    }
}