using Common.Helpers;
using Common.Ticker;
using UnityEngine.Events;

namespace Common.Managers.Advertising.AdBlocks
{
    public interface IAdBase
    {
        bool Ready { get; }
        void Init(string _AppId, string _UnitId);
        void LoadAd();
        void ShowAd(UnityAction _OnShown, UnityAction _OnClicked);
    }
    
    public abstract class AdBase : IAdBase, IUpdateTick
    {
        #region constants

        protected const string AdTypeRewarded     = "Rewarded";
        protected const string AdTypeInterstitial = "Interstitial";

        #endregion
        
        #region nonpublic members

        protected abstract string AdSource { get; }
        protected abstract string AdType   { get; }
        
        protected UnityAction OnShown;
        protected UnityAction OnClicked;
        protected string      UnitId;
        protected bool        DoLoadAdWithDelay;
        private   float       m_LoadAdDelayTimer;

        protected volatile bool        DoInvokeOnShown;
        protected volatile bool        DoInvokeOnClicked;
        
        #endregion

        #region inject

        private GlobalGameSettings GlobalGameSettings { get; }
        private ICommonTicker      CommonTicker       { get; }

        protected AdBase(
            GlobalGameSettings _GlobalGameSettings, 
            ICommonTicker      _CommonTicker)
        {
            GlobalGameSettings = _GlobalGameSettings;
            CommonTicker       = _CommonTicker;
        }

        #endregion

        #region api
        
        public virtual void Init(string _AppId, string _UnitId)
        {
            UnitId = _UnitId;
            CommonTicker.Register(this);
            LoadAd();
        }
        
        public abstract bool Ready { get; }
        public abstract void LoadAd();
        public abstract void ShowAd(UnityAction _OnShown, UnityAction _OnClicked);
        
        public void UpdateTick()
        {
            if (DoInvokeOnClicked)
            {
                Dbg.Log("Appodeal click action");
                OnClicked?.Invoke();
                DoInvokeOnClicked = false;
            }
            if (DoInvokeOnShown)
            {
                Dbg.Log("Appodeal shown action");
                OnShown?.Invoke();
                LoadAd();
                DoInvokeOnShown = false;
            }
            if (!DoLoadAdWithDelay)
                return;
            m_LoadAdDelayTimer += CommonTicker.DeltaTime;
            if (m_LoadAdDelayTimer < GlobalGameSettings.adsLoadDelay) 
                return;
            LoadAd();
            m_LoadAdDelayTimer = 0f;
            DoLoadAdWithDelay = false;
        }

        #endregion

        #region nonpublic methods

        protected virtual void OnAdLoaded()
        {
            Dbg.Log($"{AdSource}: {AdType} loaded");
        }
        
        protected virtual void OnAdFailedToLoad()
        {
            Dbg.LogWarning($"{AdSource}: {AdType} load failed");
            DoLoadAdWithDelay = true;
        }
        
        protected virtual void OnAdShown()
        {
            Dbg.Log($"{AdSource}: {AdType} shown");
            DoInvokeOnShown = true;
        }
        
        protected virtual void OnAdFailedToShow()
        {
            Dbg.LogWarning($"{AdSource}: {AdType} show failed");
            DoLoadAdWithDelay = true;
        }
        
        protected virtual void OnAdClicked()
        {
            Dbg.Log($"{AdSource}: {AdType} clicked");
            DoInvokeOnClicked = true;
        }

        #endregion
    }
}