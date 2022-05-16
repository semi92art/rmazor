using System;
using UnityEngine.Events;
using GoogleMobileAds.Api;

namespace Common.Managers.Advertising
{
    public interface IAdMobInterstitialAd : IAdBase { }
    
    public class AdMobInterstitialAd : IAdMobInterstitialAd
    {
        private InterstitialAd m_InterstitialAd;
        private UnityAction    m_OnShown;
        
        public bool Ready => m_InterstitialAd != null && m_InterstitialAd.IsLoaded();
        
        public void Init(string _AppId, string _UnitId)
        {
            m_InterstitialAd = new InterstitialAd(_UnitId);

            m_InterstitialAd.OnAdLoaded       += OnInterstitialAdLoaded;
            m_InterstitialAd.OnAdFailedToLoad += OnInterstitialAdFailedToLoad;
            m_InterstitialAd.OnAdClosed       += OnInterstitialAdClosed;
        }

        public void ShowAd(UnityAction _OnShown)
        {
            m_OnShown = _OnShown;
            m_InterstitialAd.Show();
        }

        public void LoadAd()
        {
            var adRequest = new AdRequest.Builder().Build();
            m_InterstitialAd.LoadAd(adRequest);
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
            m_OnShown?.Invoke();
            LoadAd();
        }
    }
}