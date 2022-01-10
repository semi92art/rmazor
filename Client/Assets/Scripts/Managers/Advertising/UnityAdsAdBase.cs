using GameHelpers;
using Ticker;
using UnityEngine.Advertisements;
using UnityEngine.Events;

namespace Managers.Advertising
{
    public interface IUnityAdsAd : IAdBase, IUnityAdsLoadListener, IUnityAdsShowListener { }
    
    public class UnityAdsAdBase : IUnityAdsAd, IUpdateTick
    {
        protected volatile bool        m_DoInvokeOnShown;
        protected          bool        m_DoLoadAdWithDelay;
        protected          float       m_LoadAdDelayTimer;
        protected          string      m_UnitId;
        protected          UnityAction m_OnShown;

        private CommonGameSettings Settings   { get; }
        private ICommonTicker      CommonTicker { get; }
        
        protected UnityAdsAdBase(CommonGameSettings _Settings,  ICommonTicker _CommonTicker)
        {
            Settings = _Settings;
            CommonTicker = _CommonTicker;
        }
        
        public bool Ready { get; protected set; }
        
        public void Init(string _UnitId)
        {
            CommonTicker.Register(this);
            m_UnitId = _UnitId;
            LoadAd();
        }

        public virtual void ShowAd(UnityAction _OnShown)
        {
            m_OnShown = _OnShown;
            Advertisement.Show(m_UnitId, this);
        }

        public virtual void OnUnityAdsAdLoaded(string _PlacementId)
        {
            Dbg.Log(nameof(OnUnityAdsAdLoaded) + ": " + _PlacementId);
            Ready = true;
        }

        public virtual void OnUnityAdsFailedToLoad(string _PlacementId, UnityAdsLoadError _Error, string _Message)
        {
            string message = string.Join(": ", 
                nameof(OnUnityAdsFailedToLoad), _PlacementId, _Message);
            Dbg.LogWarning(message);
            Ready = false;
            m_DoLoadAdWithDelay = true;
        }

        public virtual void OnUnityAdsShowFailure(string _PlacementId, UnityAdsShowError _Error, string _Message)
        {
            string message = string.Join(": ", 
                nameof(OnUnityAdsShowFailure), _PlacementId, _Message);
            Dbg.Log(message);
            Ready = false;
            LoadAd();
        }

        public virtual void OnUnityAdsShowStart(string _PlacementId)
        {
            Dbg.Log(nameof(OnUnityAdsShowStart) + ": " + _PlacementId);
            Ready = false;
        }

        public virtual void OnUnityAdsShowClick(string _PlacementId)
        {
            Dbg.Log(nameof(OnUnityAdsShowClick) + ": " + _PlacementId);
            Ready = false;
            LoadAd();
        }

        public virtual void OnUnityAdsShowComplete(string _PlacementId, UnityAdsShowCompletionState _ShowCompletionState)
        {
            string message = string.Join(": ", 
                GetType().Name, nameof(OnUnityAdsShowComplete), _PlacementId, _ShowCompletionState);
            Dbg.Log(message);
            m_DoInvokeOnShown = true;
            Ready = false;
            LoadAd();
        }
        
        public virtual void LoadAd()
        {
            Ready = false;
            Advertisement.Load(m_UnitId, this);
        }

        public void UpdateTick()
        {
            if (m_DoInvokeOnShown)
            {
                Dbg.Log("m_OnShown?.Invoke()");
                m_OnShown?.Invoke();
                m_DoInvokeOnShown = false;
            }

            if (m_DoLoadAdWithDelay)
            {
                m_LoadAdDelayTimer += CommonTicker.DeltaTime;
                if (!(m_LoadAdDelayTimer > Settings.AdsLoadDelay)) 
                    return;
                LoadAd();
                m_LoadAdDelayTimer = 0f;
                m_DoLoadAdWithDelay = false;
            }
        }
    }
}