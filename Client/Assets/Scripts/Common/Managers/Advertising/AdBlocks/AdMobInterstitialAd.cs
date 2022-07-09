#if ADMOB_API

using System;
using UnityEngine.Events;
using GoogleMobileAds.Api;


namespace Common.Managers.Advertising.AdBlocks
{
    public interface IAdMobInterstitialAd : IAdBase { }
    
    public class AdMobInterstitialAd : AdBase, IAdMobInterstitialAd
    {
        private InterstitialAd m_InterstitialAd;
        
        public bool Ready => m_InterstitialAd != null && m_InterstitialAd.IsLoaded();
        
        public void Init(string _AppId, string _UnitId)
        {
            m_InterstitialAd = new InterstitialAd(_UnitId);
            m_InterstitialAd.OnAdLoaded              += OnInterstitialAdLoaded;
            m_InterstitialAd.OnAdFailedToLoad        += OnInterstitialAdFailedToLoad;
            m_InterstitialAd.OnAdClosed              += OnInterstitialAdClosed;
            m_InterstitialAd.OnAdDidRecordImpression += OnAdDidRecordImpression;
            m_InterstitialAd.OnPaidEvent             += OnInterstitialAdPaidEvent;
        }
        
        public void ShowAd(UnityAction _OnShown, UnityAction _OnClicked)
        {
            OnShown = _OnShown;
            OnClicked = _OnClicked;
            m_InterstitialAd.Show();
        }

        public void LoadAd()
        {
            var adRequest = new AdRequest.Builder().Build();
            m_InterstitialAd.LoadAd(adRequest);
        }
        
        private void OnInterstitialAdLoaded(object _Sender, EventArgs _E)
        {
            Dbg.Log("AdMob: " + nameof(OnInterstitialAdLoaded));
        }
        
        private void OnInterstitialAdFailedToLoad(object _Sender, AdFailedToLoadEventArgs _E)
        {
            Dbg.Log("AdMob: " + nameof(OnInterstitialAdFailedToLoad));
            LoadAd();
        }
        
        private void OnInterstitialAdClosed(object _Sender, EventArgs _E)
        {
            Dbg.Log("AdMob: " + nameof(OnInterstitialAdClosed));
            OnShown?.Invoke();
            LoadAd();
        }
        
        private void OnAdDidRecordImpression(object _Sender, EventArgs _E)
        {
            Dbg.Log("AdMob: " + nameof(OnAdDidRecordImpression));

        }
        
        private void OnInterstitialAdPaidEvent(object _Sender, AdValueEventArgs _E)
        {
            Dbg.Log("AdMob: " + nameof(OnInterstitialAdPaidEvent));
            OnClicked?.Invoke();
        }
    }
}

#endif