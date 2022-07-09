#if UNITY_ADS_API

using Common.Helpers;
using Common.Ticker;
using UnityEngine.Advertisements;
using UnityEngine.Events;

namespace Common.Managers.Advertising
{
    public interface IUnityAdsAd : IAdBase, IUnityAdsLoadListener, IUnityAdsShowListener { }

    public class UnityAdsAdBase : AdBase, IUnityAdsAd, IUpdateTick
    {
        #region nonpublic members

        protected string UnitId;
        protected bool   DoInvokeOnShown;
        private   bool   m_DoInvokeOnClicked;
        private   bool   m_DoLoadAdWithDelay;
        private   float  m_LoadAdDelayTimer;

        #endregion

        #region inject

        private CommonGameSettings Settings     { get; }
        private ICommonTicker      CommonTicker { get; }

        protected UnityAdsAdBase(CommonGameSettings _Settings, ICommonTicker _CommonTicker)
        {
            Settings = _Settings;
            CommonTicker = _CommonTicker;
        }

        #endregion

        #region api

        public bool Ready { get; protected set; }

        public void Init(string _AppId, string _UnitId)
        {
            CommonTicker.Register(this);
            UnitId = _UnitId;
            LoadAd();
        }

        public virtual void ShowAd(UnityAction _OnShown, UnityAction _OnClicked)
        {
            OnShown = _OnShown;
            OnClicked = _OnClicked;
            Advertisement.Show(UnitId, this);
        }

        public void UpdateTick()
        {
            if (m_DoInvokeOnClicked)
            {
                OnClicked?.Invoke();
                m_DoInvokeOnClicked = false;
            }

            if (DoInvokeOnShown)
            {
                OnShown?.Invoke();
                DoInvokeOnShown = false;
            }

            if (!m_DoLoadAdWithDelay)
                return;
            m_LoadAdDelayTimer += CommonTicker.DeltaTime;
            if (!(m_LoadAdDelayTimer > Settings.adsLoadDelay))
                return;
            LoadAd();
            m_LoadAdDelayTimer = 0f;
            m_DoLoadAdWithDelay = false;
        }

        #endregion

        #region event methods methods

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
            m_DoInvokeOnClicked = true;
        }

        public virtual void OnUnityAdsShowComplete(string                      _PlacementId,
                                                   UnityAdsShowCompletionState _ShowCompletionState)
        {
            string message = string.Join(": ",
                GetType().Name, nameof(OnUnityAdsShowComplete), _PlacementId, _ShowCompletionState);
            Dbg.Log(message);
            DoInvokeOnShown = true;
            Ready = false;
            LoadAd();
        }

        public virtual void LoadAd()
        {
            Ready = false;
            Advertisement.Load(UnitId, this);
        }

        #endregion
    }
}

#endif