// using AudienceNetwork;

using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Managers.Advertising
{
    public interface IFacebookAd : IAdBase { }
    
    public class FacebookInterstitialAd : MonoBehaviour, IFacebookAd
    {
        #region nonpublic members

        // private InterstitialAd m_Ad;
        private bool           m_IsLoaded;
        private bool           m_DidClosed;
        private UnityAction    m_OnShown;

        #endregion

        #region api

        public bool Ready => m_IsLoaded;
        
        public void Init(string _PlacementId)
        {
            // m_Ad = new InterstitialAd(_PlacementId);
            // m_Ad.Register(gameObject);
            // m_Ad.InterstitialAdDidLoad           = OnDidLoad;
            // m_Ad.InterstitialAdDidFailWithError  = OnDidFailWithError;
            // m_Ad.InterstitialAdWillLogImpression = OnWillLogImpression;
            // m_Ad.interstitialAdDidClick          = OnDidClick;
            // m_Ad.InterstitialAdDidClose          = OnDidClose;
            // m_Ad.InterstitialAdActivityDestroyed = OnActivityDestroyed;
        }

        public void ShowAd(UnityAction _OnShown)
        {
            if (m_IsLoaded)
            {
                // m_Ad.Show();
                m_IsLoaded = false;
                m_DidClosed = false;
                m_OnShown = _OnShown;
            }
            else Dbg.LogWarning($"{GetType().Name} not loaded");
        }

        public void LoadAd()
        {
            // m_Ad.LoadAd();
        }
        
        #endregion

        #region nonpublic methods


        #endregion

        #region event methods

        private void OnDidLoad()
        {
            m_IsLoaded = true;
        }
        
        private void OnDidFailWithError(string _Error)
        {
            Dbg.LogWarning($"Failed to load {GetType().Name}: " + _Error);
        }
        
        private void OnWillLogImpression()
        {
            Dbg.Log($"{GetType().Name} logged impression");
        }
        
        private void OnDidClick()
        {
            Dbg.Log($"{GetType().Name} was clicked");
        }
        
        private void OnDidClose()
        {
            Dbg.Log($"{GetType().Name} was closed");
            // m_Ad?.Dispose();
            m_DidClosed = true;
            m_OnShown?.Invoke();
            Coroutines.Run(Coroutines.Delay(
                3f,
                LoadAd));
        }
        
        private void OnActivityDestroyed()
        {
            if (!m_DidClosed)
            {
                Dbg.Log($"{GetType().Name} activity destroyed without being closed first");
                // m_Ad?.Dispose();
            }
            LoadAd();
        }

        #endregion
    }
}