using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DI.Extensions;
using GoogleMobileAds.Api;
using UnityEngine.Events;
using Utils;

namespace Managers
{
    public class GoogleAdMobAdsManager : AdsManagerBase
    {

        #region nonpublic members

        private RewardedAd     m_RewardedAd;
        private InterstitialAd m_InterstitialAd;
        private AdRequest      m_AdRequest;
        private bool           m_ShowAds;

        #endregion

        #region inject

        public GoogleAdMobAdsManager(IShopManager _ShopManager) : base(_ShopManager) { }

        #endregion

        #region api
        
        public override bool RewardedAdReady     => m_RewardedAd.IsLoaded();
        public override bool InterstitialAdReady => m_InterstitialAd.IsLoaded();
        
        public override void ShowRewardedAd(UnityAction _OnPaid)
        {
            m_OnRewardedAdShown = _OnPaid;
            if (RewardedAdReady)
                m_RewardedAd.Show();
            else
                m_RewardedAd.LoadAd(m_AdRequest);
        }
        
        public override void ShowInterstitialAd(UnityAction _OnShown)
        {
            m_OnInterstitialShown = _OnShown;
            if (InterstitialAdReady)
                m_InterstitialAd.Show();
            else
                m_InterstitialAd.LoadAd(m_AdRequest);
        }
        
        #endregion

        #region nonpublic methods

        protected override void InitConfigs()
        {
            new RequestConfiguration.Builder()
                .SetTestDeviceIds(GetTestDeviceIds())
                .build();
            MobileAds.Initialize(_InitStatus => { });
            m_AdRequest = new AdRequest.Builder().Build();
        }

        protected override void InitRewardedAd()
        {
            string rewardedAdId = GetAdsNodeValue("admob", "interstitial");
            m_RewardedAd = new RewardedAd(rewardedAdId);
            m_RewardedAd.LoadAd(m_AdRequest);
            m_RewardedAd.OnAdLoaded += OnRewardedAdLoaded;
            m_RewardedAd.OnAdFailedToLoad += OnRewardedAdFailedToLoad;
            m_RewardedAd.OnPaidEvent += OnRewardedAdPaidEvent;
            m_RewardedAd.OnAdClosed += OnRewardedAdClosed;
        }
        
        protected override void InitInterstitialAd()
        {
            string interstitialAdId = GetAdsNodeValue("admob", "reward");
            m_InterstitialAd = new InterstitialAd(interstitialAdId);
            m_InterstitialAd.LoadAd(m_AdRequest);
            m_InterstitialAd.OnAdLoaded += OnInterstitialAdLoaded;
            m_InterstitialAd.OnAdFailedToLoad += OnInterstitialAdFailedToLoad;
            m_InterstitialAd.OnAdClosed += OnInterstitialAdClosed;
        }
        
        private List<string> GetTestDeviceIds()
        {
            return m_Ads.Elements("test_device").Select(_El => _El.Value).ToList();
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
            m_OnRewardedAdShown?.Invoke();
        }
        
        private void OnRewardedAdClosed(object _Sender, EventArgs _E)
        {
            Dbg.Log(nameof(OnRewardedAdClosed).WithSpaces());
            m_RewardedAd.LoadAd(m_AdRequest);
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
            m_InterstitialAd.LoadAd(m_AdRequest);
        }
        
        #endregion

    }
}