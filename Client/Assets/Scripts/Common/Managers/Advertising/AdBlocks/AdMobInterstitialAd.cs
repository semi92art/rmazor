#if ADMOB_API

using System;
using System.Text;
using Common.Helpers;
using Common.Ticker;
using UnityEngine.Events;
using GoogleMobileAds.Api;

namespace Common.Managers.Advertising.AdBlocks
{
    public interface IAdMobInterstitialAd : IInterstitialAdBase { }
    
    public class AdMobInterstitialAd : InterstitialAdBase, IAdMobInterstitialAd
    {
        #region nonpublic members
        
        protected override string AdSource => AdvertisingNetworks.Admob;
        protected override string AdType   => AdTypeInterstitial;
        
        private InterstitialAd m_InterstitialAd;

        #endregion

        #region inject

        private AdMobInterstitialAd(
            GlobalGameSettings _GameSettings,
            ICommonTicker      _CommonTicker)
            : base(_GameSettings, _CommonTicker) { }

        #endregion

        #region api
        
        public override bool Ready => m_InterstitialAd != null && m_InterstitialAd.IsLoaded();
        
        public override void Init(string _AppId, string _UnitId)
        {
            m_InterstitialAd = new InterstitialAd(_UnitId);
            m_InterstitialAd.OnAdLoaded              += OnInterstitialAdLoaded;
            m_InterstitialAd.OnAdFailedToLoad        += OnInterstitialAdFailedToLoad;
            m_InterstitialAd.OnAdClosed              += OnInterstitialAdClosed;
            m_InterstitialAd.OnAdDidRecordImpression += OnAdDidRecordImpression;
            m_InterstitialAd.OnPaidEvent             += OnInterstitialAdPaidEvent;
            base.Init(_AppId, _UnitId);
        }
        
        public override void ShowAd(UnityAction _OnShown, UnityAction _OnClicked)
        {
            OnShown = _OnShown;
            OnClicked = _OnClicked;
            m_InterstitialAd.Show();
        }

        public override void LoadAd()
        {
            var adRequest = new AdRequest.Builder().Build();
            m_InterstitialAd.LoadAd(adRequest);
        }

        #endregion

        #region nonpublic methods
        
        private void OnInterstitialAdLoaded(object _Sender, EventArgs _E)
        {
            OnAdLoaded();
        }
        
        private void OnInterstitialAdFailedToLoad(object _Sender, AdFailedToLoadEventArgs _E)
        {
            OnAdFailedToLoad();
            var sb = new StringBuilder();
            sb.AppendLine("Code: " + _E.LoadAdError.GetCode());
            sb.AppendLine("Message: " + _E.LoadAdError.GetMessage());
            sb.AppendLine("Domain: " + _E.LoadAdError.GetDomain());
            sb.AppendLine("Unit Id: " + UnitId);
            Dbg.LogWarning(sb);
        }
        
        private void OnInterstitialAdClosed(object _Sender, EventArgs _E)
        {
            OnAdShown();
        }
        
        private void OnAdDidRecordImpression(object _Sender, EventArgs _E)
        {
            Dbg.Log("AdMob: Interstitial record impression");
        }
        
        private void OnInterstitialAdPaidEvent(object _Sender, AdValueEventArgs _E)
        {
            OnAdClicked();
            var sb = new StringBuilder();
            sb.AppendLine("Precision: " + _E.AdValue.Precision);
            sb.AppendLine("Value: " + _E.AdValue.Value);
            sb.AppendLine("CurrencyCode: " + _E.AdValue.CurrencyCode);         
            Dbg.Log(sb);
        }

        #endregion
    }
}

#endif