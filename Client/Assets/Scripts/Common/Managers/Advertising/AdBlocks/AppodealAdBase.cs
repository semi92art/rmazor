#if APPODEAL_3
using AppodealStack.Monetization.Api;
using Common.Helpers;
using Common.Ticker;
using UnityEngine.Events;

namespace Common.Managers.Advertising.AdBlocks
{
    public abstract class AppodealAdBase : AdBase, IUpdateTick
    {
        #region nonpublic members
        
        protected abstract int ShowStyle { get; }
        protected abstract int AdType { get; }
        
        protected          bool  DoLoadAdWithDelay;
        protected volatile bool  DoInvokeOnShown;
        protected volatile bool  DoInvokeOnClicked;
        private            float m_LoadAdDelayTimer;
        
        #endregion

        #region inject

        private GlobalGameSettings GameSettings { get; }
        private ICommonTicker      CommonTicker { get; }

        protected AppodealAdBase(
            GlobalGameSettings _GameSettings,
            ICommonTicker      _CommonTicker)
        {
            GameSettings = _GameSettings;
            CommonTicker = _CommonTicker;
        }

        #endregion

        #region api
        
        public bool Ready => Appodeal.IsLoaded(AdType);

        public virtual void Init(string _AppId, string _UnitId)
        {
            CommonTicker.Register(this);
            LoadAd();
        }

        public void LoadAd()
        {
            Dbg.Log($"Try to load appodeal ad, type: {AdType}");
            Appodeal.Cache(AdType);
        }

        public void ShowAd(UnityAction _OnShown, UnityAction _OnClicked)
        {
            OnShown = _OnShown;
            OnClicked = _OnClicked;
            if (!Ready) 
                return;
            Appodeal.Show(ShowStyle, "default");
        }

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
            if (m_LoadAdDelayTimer < GameSettings.adsLoadDelay) 
                return;
            LoadAd();
            m_LoadAdDelayTimer = 0f;
            DoLoadAdWithDelay = false;
        }

        #endregion

        #region nonpublic methods

        #endregion
    }
}
#endif