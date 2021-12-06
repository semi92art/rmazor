using DI.Extensions;
using UnityEngine.Advertisements;
using UnityEngine.Events;
using Utils;

namespace Managers
{
    public interface IUnityAdsAd
    {
        bool Ready { get; }
        void Init(string _UnitId);
        void ShowAd(UnityAction _OnShown);
    }
    
    public class UnityAdsAdBase : IUnityAdsAd, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        protected string      m_UnitId;
        protected UnityAction m_OnShown;
        
        public bool Ready { get; protected set; }
        
        public void Init(string _UnitId)
        {
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
            Dbg.Log(nameof(OnUnityAdsAdLoaded).WithSpaces() + ": " + _PlacementId);
            Ready = true;
        }

        public virtual void OnUnityAdsFailedToLoad(string _PlacementId, UnityAdsLoadError _Error, string _Message)
        {
            string message = string.Join(": ", 
                nameof(OnUnityAdsFailedToLoad).WithSpaces(), _PlacementId, _Message);
            Dbg.Log(message);
            Ready = false;
            LoadAd();
        }

        public virtual void OnUnityAdsShowFailure(string _PlacementId, UnityAdsShowError _Error, string _Message)
        {
            string message = string.Join(": ", 
                nameof(OnUnityAdsShowFailure).WithSpaces(), _PlacementId, _Message);
            Dbg.Log(message);
            Ready = false;
            LoadAd();
        }

        public virtual void OnUnityAdsShowStart(string _PlacementId)
        {
            Dbg.Log(nameof(OnUnityAdsShowStart).WithSpaces() + ": " + _PlacementId);
            Ready = false;
        }

        public virtual void OnUnityAdsShowClick(string _PlacementId)
        {
            Dbg.Log(nameof(OnUnityAdsShowClick).WithSpaces() + ": " + _PlacementId);
            Ready = false;
        }

        public virtual void OnUnityAdsShowComplete(string _PlacementId, UnityAdsShowCompletionState _ShowCompletionState)
        {
            string message = string.Join(": ", 
                nameof(OnUnityAdsShowComplete).WithSpaces(), _PlacementId, _ShowCompletionState);
            Dbg.Log(message);
            m_OnShown?.Invoke();
            Ready = false;
            LoadAd();
        }
        
        protected virtual void LoadAd()
        {
            Ready = false;
            Advertisement.Load(m_UnitId, this);
        }
    }
}