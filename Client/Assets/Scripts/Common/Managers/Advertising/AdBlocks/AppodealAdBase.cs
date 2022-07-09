#if APPODEAL_3
using AppodealStack.Monetization.Api;
using Common.Helpers;
using Common.Ticker;
using UnityEngine.Events;

namespace Common.Managers.Advertising.AdBlocks
{
    public abstract class AppodealAdBase : AdBase, IUpdateTick
    {
        #region constants

        private const string DefaultPlacement = "default";
        
        #endregion

        #region nonpublic members
        
        protected abstract int ShowStyle { get; }
        protected abstract int AdType { get; }
        
        protected          bool  DoLoadAdWithDelay;
        protected volatile bool  DoInvokeOnShown;
        protected volatile bool  DoInvokeOnClicked;
        private            float m_LoadAdDelayTimer;
        
        #endregion

        #region inject
        
        private CommonGameSettings Settings     { get; }
        private ICommonTicker      CommonTicker { get; }

        protected AppodealAdBase(
            CommonGameSettings _Settings,
            ICommonTicker      _CommonTicker)
        {
            Settings     = _Settings;
            CommonTicker = _CommonTicker;
        }

        #endregion

        #region api
        
        public bool Ready
        {
            get
            {
                Dbg.Log($"Appodeal ad ready status: loaded: {Appodeal.IsLoaded(AdType)}," +
                        $" canShow: {Appodeal.CanShow(AdType, DefaultPlacement)}");
                return Appodeal.IsLoaded(AdType)
                       && Appodeal.CanShow(AdType, DefaultPlacement);
            }
        }

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
            Dbg.Log("Attempt to show appodeal ad, type: " + AdType + ", show style: " + ShowStyle);
            Appodeal.Show(ShowStyle, DefaultPlacement);
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
                DoInvokeOnShown = false;
            }
            if (!DoLoadAdWithDelay)
                return;
            m_LoadAdDelayTimer += CommonTicker.DeltaTime;
            if (!(m_LoadAdDelayTimer > Settings.adsLoadDelay)) 
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