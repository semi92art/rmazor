﻿#if UNITY_ADS_API

using Common.Helpers;
using Common.Ticker;
using UnityEngine.Advertisements;

namespace Common.Managers.Advertising
{
    public interface IUnityAdsRewardedAd : IUnityAdsAd { }
    
    public class UnityAdsRewardedAd : UnityAdsAdBase, IUnityAdsRewardedAd
    {
        public UnityAdsRewardedAd(CommonGameSettings _Settings, ICommonTicker _CommonTicker)
            : base(_Settings, _CommonTicker) { }
        
        public override void OnUnityAdsShowComplete(string _PlacementId, UnityAdsShowCompletionState _ShowCompletionState)
        {
            if (!_PlacementId.Equals(UnitId)
                || !_ShowCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED)) 
                return;
            string message = string.Join(": ", 
                GetType().Name, nameof(OnUnityAdsShowComplete), _PlacementId, _ShowCompletionState);
            Dbg.Log(message);
            DoInvokeOnShown = true;
            Ready = false;
            LoadAd();
        }
    }
}

#endif