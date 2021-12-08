using DI.Extensions;
using Ticker;
using UnityEngine.Advertisements;
using UnityEngine.Events;
using Utils;

namespace Managers.Advertising
{
    public interface IUnityAdsAd : IUnityAdsLoadListener, IUnityAdsShowListener
    {
        bool Ready { get; }
        void Init(string _UnitId);
        void ShowAd(UnityAction _OnShown);
        void LoadAd();
    }
    
    public class UnityAdsAdBase : IUnityAdsAd, IUpdateTick
    {
        protected bool        m_DoInvokeOnShown;
        protected string      m_UnitId;
        protected UnityAction m_OnShown;

        private   IViewGameTicker GameTicker { get; }
        
        protected UnityAdsAdBase(IViewGameTicker _GameTicker)
        {
            GameTicker = _GameTicker;
        }
        
        public bool Ready { get; protected set; }
        
        public void Init(string _UnitId)
        {
            GameTicker.Register(this);
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
            Dbg.Log(message);
            Ready = false;
            LoadAd();
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
                m_OnShown?.Invoke();
                m_DoInvokeOnShown = false;
            }
        }
    }
}