#if UNITY_ADS_API
using System.Text;
using Common.Helpers;
using Common.Ticker;
using UnityEngine.Advertisements;
using UnityEngine.Events;

namespace Common.Managers.Advertising.AdBlocks
{
    public interface IUnityAdsAd :
        IAdBase,
        IUnityAdsLoadListener, 
        IUnityAdsShowListener { }

    public abstract class UnityAdsAdBase : AdBase, IUnityAdsAd
    {
        #region nonpublic members

        protected override string AdSource => AdvertisingNetworks.UnityAds;

        protected bool IsReady;

        #endregion

        #region inject

        protected UnityAdsAdBase(
            GlobalGameSettings _Settings,
            ICommonTicker      _CommonTicker)
            : base(_Settings, _CommonTicker) { }

        #endregion

        #region api

        public override bool Ready => IsReady;

        public override void LoadAd()
        {
            IsReady = false;
            Advertisement.Load(UnitId, this);
        }

        public override void ShowAd(UnityAction _OnShown, UnityAction _OnClicked)
        {
            OnShown = _OnShown;
            OnClicked = _OnClicked;
            Advertisement.Show(UnitId, this);
        }

        #endregion

        #region event methods methods

        public virtual void OnUnityAdsAdLoaded(string _PlacementId)
        {
            IsReady = true;
            OnAdLoaded();
        }

        public virtual void OnUnityAdsFailedToLoad(string _PlacementId, UnityAdsLoadError _Error, string _Message)
        {
            IsReady = false;
            OnAdFailedToLoad();
            var sb = new StringBuilder();
            sb.AppendLine($"message: {_Message}");
            sb.AppendLine($"error: {_Error}");
            Dbg.LogWarning(sb);
        }

        public virtual void OnUnityAdsShowFailure(string _PlacementId, UnityAdsShowError _Error, string _Message)
        {
            IsReady = false;
            OnAdFailedToShow();
            var sb = new StringBuilder();
            sb.AppendLine($"message: {_Message}");
            sb.AppendLine($"error: {_Error}");
            Dbg.LogWarning(sb);
        }

        public virtual void OnUnityAdsShowStart(string _PlacementId)
        {
            Dbg.Log($"Unity Ads: {AdType} ad show start");
            IsReady = false;
        }

        public virtual void OnUnityAdsShowClick(string _PlacementId)
        {
            IsReady = false;
            OnAdClicked();
        }

        public virtual void OnUnityAdsShowComplete(
            string                      _PlacementId,
            UnityAdsShowCompletionState _ShowCompletionState)
        {
            IsReady = false;
            OnAdShown();
            Dbg.Log($"Completion state: {_ShowCompletionState}");
        }

        #endregion
    }
}

#endif