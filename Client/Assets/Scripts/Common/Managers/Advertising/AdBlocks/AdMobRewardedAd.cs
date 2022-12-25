#if ADMOB_API

using System;
using System.Text;
using Common.Helpers;
using UnityEngine.Events;
using GoogleMobileAds.Api;
using mazing.common.Runtime;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Ticker;

namespace Common.Managers.Advertising.AdBlocks
{
    public interface IAdMobRewardedAd : IRewardedAdBase { }
    
    public class AdMobRewardedAd : RewardedAdBase, IAdMobRewardedAd
    {
        #region nonpublic members

        protected override string AdSource => AdvertisingNetworks.Admob;
        protected override string AdType   => AdTypeRewarded;
        
        private RewardedAd m_RewardedAd;
        
        
        #endregion

        #region inject
        
        public AdMobRewardedAd(
            GlobalGameSettings _GlobalGameSettings,
            ICommonTicker      _CommonTicker) 
            : base(_GlobalGameSettings, _CommonTicker) { }
        
        #endregion

        #region api
        
        public override bool Ready => m_RewardedAd != null && m_RewardedAd.IsLoaded();
        
        public override void Init(string _AppId, string _UnitId)
        {
            m_RewardedAd = new RewardedAd(_UnitId);
            m_RewardedAd.OnAdLoaded              += OnRewardedAdLoaded;
            m_RewardedAd.OnAdFailedToLoad        += OnRewardedAdFailedToLoad;
            m_RewardedAd.OnAdFailedToShow        += OnRewardedAdFailedToShow;
            m_RewardedAd.OnPaidEvent             += OnRewardedAdPaidEvent;
            m_RewardedAd.OnAdClosed              += OnRewardedAdClosed;
            m_RewardedAd.OnUserEarnedReward      += OnRewardedAdUserEarnedReward;
            m_RewardedAd.OnAdDidRecordImpression += OnRewardedAdDidRecordImpression;
            base.Init(_AppId, _UnitId);
        }

        public override void ShowAd(
            UnityAction _OnShown,
            UnityAction _OnClicked,
            UnityAction _OnReward,
            UnityAction _OnClosed,
            UnityAction _OnFailedToShow)
        {
            OnShown        = _OnShown;
            OnClicked      = _OnClicked;
            OnReward       = _OnReward;
            OnClosed       = _OnClosed;
            OnFailedToShow = _OnFailedToShow;
            m_RewardedAd.Show();
        }

        public override void LoadAd()
        {
            var adRequest = new AdRequest.Builder().Build();
            m_RewardedAd.LoadAd(adRequest);
        }

        #endregion

        #region nonpublic methods
        
        private void OnRewardedAdLoaded(object _Sender, EventArgs _E)
        {
            OnAdLoaded();
        }
        
        private void OnRewardedAdFailedToLoad(object _Sender, AdFailedToLoadEventArgs _E)
        {
            OnAdFailedToLoad();
            var sb = new StringBuilder();
            sb.AppendLine("Code: " + _E.LoadAdError.GetCode());
            sb.AppendLine("Message: " + _E.LoadAdError.GetMessage());
            sb.AppendLine("Domain: " + _E.LoadAdError.GetDomain());
            sb.AppendLine("Unit Id: " + UnitId);
            Dbg.LogWarning(sb);
        }
        
        private void OnRewardedAdFailedToShow(object _Sender, AdErrorEventArgs _E)
        {
            OnAdFailedToShow();
            var sb = new StringBuilder();
            sb.AppendLine("Code: " + _E.AdError.GetCode());
            sb.AppendLine("Message: " + _E.AdError.GetMessage());
            sb.AppendLine("Domain: " + _E.AdError.GetDomain());
            sb.AppendLine("Unit Id: " + UnitId);
            Dbg.LogWarning(sb);
        }
        
        private void OnRewardedAdPaidEvent(object _Sender, AdValueEventArgs _E)
        {
            OnAdClicked();
        }
        
        private void OnRewardedAdClosed(object _Sender, EventArgs _E)
        {
            OnAdClosed();
        }
        
        private void OnRewardedAdDidRecordImpression(object _Sender, EventArgs _E)
        {
            OnAdShown();
        }

        private void OnRewardedAdUserEarnedReward(object _Sender, Reward _E)
        {
            OnAdRewardGot();
        }

        #endregion
    }
}

#endif