using System;
using UnityEngine.Events;
using GoogleMobileAds.Api;

namespace Common.Managers.Advertising
{
    public interface IAdMobRewardedAd : IAdBase { }
    
    public class AdMobRewardedAd : IAdMobRewardedAd
    {
        private RewardedAd  m_RewardedAd;
        private UnityAction m_OnShown;
        
        public bool Ready => m_RewardedAd != null && m_RewardedAd.IsLoaded();
        public void Init(string _UnitId)
        {
            m_RewardedAd = new RewardedAd(_UnitId);
            m_RewardedAd.OnAdLoaded              += OnRewardedAdLoaded;
            m_RewardedAd.OnAdFailedToLoad        += OnRewardedAdFailedToLoad;
            m_RewardedAd.OnPaidEvent             += OnRewardedAdPaidEvent;
            m_RewardedAd.OnAdClosed              += OnRewardedAdClosed;
            m_RewardedAd.OnUserEarnedReward      += OnRewardedAdUserEarnedReward;
            m_RewardedAd.OnAdDidRecordImpression += OnRewardedAdDidRecordImpression;
            LoadAd();
        }

        public void ShowAd(UnityAction _OnShown)
        {
            m_OnShown = _OnShown;
            m_RewardedAd.Show();
        }

        public void LoadAd()
        {
            var adRequest = new AdRequest.Builder().Build();
            m_RewardedAd.LoadAd(adRequest);
        }
        
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
            m_OnShown?.Invoke();
            LoadAd();
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
    }
}