#if UNITY_ADS_API

using Common.Helpers;
using Common.Ticker;
using UnityEngine.Advertisements;

namespace Common.Managers.Advertising.AdBlocks
{
    public interface IUnityAdsRewardedAd : IUnityAdsAd { }
    
    public class UnityAdsRewardedAd : UnityAdsAdBase, IUnityAdsRewardedAd
    {
        protected override string AdType => AdTypeRewarded;
        
        public UnityAdsRewardedAd(GlobalGameSettings _Settings, ICommonTicker _CommonTicker)
            : base(_Settings, _CommonTicker) { }
        
        public override void OnUnityAdsShowComplete(
            string                      _PlacementId,
            UnityAdsShowCompletionState _ShowCompletionState)
        {
            if (!_PlacementId.Equals(UnitId)) 
                return;
            IsReady = false;
            OnAdShown();
            Dbg.Log($"Completion state: {_ShowCompletionState}");
        }
    }
}

#endif