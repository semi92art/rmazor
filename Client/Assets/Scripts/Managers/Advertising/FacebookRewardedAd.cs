// using AudienceNetwork;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Managers.Advertising
{
    public class FacebookRewardedAd : MonoBehaviour, IFacebookAd
    {
        #region nonpublic members

        // private RewardedVideoAd m_Ad;
        private bool            m_IsLoaded;
        private bool            m_DidClosed;
        private UnityAction     m_OnShown;

        #endregion

        #region api

        public bool Ready => m_IsLoaded;
        
        public void Init(string _PlacementId)
        {
            // m_Ad = new RewardedVideoAd(_PlacementId);
            // m_Ad.Register(gameObject);
            // m_Ad.RewardedVideoAdDidLoad           = OnDidLoad;
            // m_Ad.RewardedVideoAdDidFailWithError  = OnDidFailWithError;
            // m_Ad.RewardedVideoAdWillLogImpression = OnWillLogImpression;
            // m_Ad.RewardedVideoAdDidClick          = OnDidClick;
            // m_Ad.RewardedVideoAdDidClose          = OnDidClose;
            // m_Ad.RewardedVideoAdActivityDestroyed = OnActivityDestroyed;
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