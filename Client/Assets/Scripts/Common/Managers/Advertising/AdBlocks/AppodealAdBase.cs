#if APPODEAL_3
using AppodealStack.Monetization.Api;
using Common.Helpers;
using Common.Ticker;
using UnityEngine.Events;

namespace Common.Managers.Advertising.AdBlocks
{
    public abstract class AppodealAdBase : AdBase
    {
        #region nonpublic members

        protected override string AdSource   => AdvertisingNetworks.Appodeal;
        protected abstract int    ShowStyle  { get; }
        protected abstract int    AppoAdType { get; }
        
        #endregion

        #region inject

        protected AppodealAdBase(
            GlobalGameSettings _GameSettings,
            ICommonTicker      _CommonTicker) 
            : base(_GameSettings, _CommonTicker) { }

        #endregion

        #region api
        
        public override bool Ready => Appodeal.IsLoaded(AppoAdType);
        
        public override void LoadAd()
        {
            Dbg.Log($"Try to load appodeal ad, type: {AppoAdType}");
            Appodeal.Cache(AppoAdType);
        }

        public override void ShowAd(UnityAction _OnShown, UnityAction _OnClicked)
        {
            OnShown = _OnShown;
            OnClicked = _OnClicked;
            if (!Ready) 
                return;
            Appodeal.Show(ShowStyle, "default");
        }

        #endregion

        #region nonpublic methods

        #endregion
    }
}
#endif