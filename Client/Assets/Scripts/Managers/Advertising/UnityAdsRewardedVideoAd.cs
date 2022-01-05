using GameHelpers;
using Ticker;
using UnityEngine.Advertisements;

namespace Managers.Advertising
{
    public interface IUnityAdsRewardedAd : IUnityAdsAd { }
    
    public class UnityAdsRewardedAd : UnityAdsAdBase, IUnityAdsRewardedAd
    {
        public UnityAdsRewardedAd(CommonGameSettings _Settings, ICommonTicker _CommonTicker)
            : base(_Settings, _CommonTicker) { }
        
        public override void OnUnityAdsShowComplete(string _PlacementId, UnityAdsShowCompletionState _ShowCompletionState)
        {
            if (!_PlacementId.Equals(m_UnitId)
                || !_ShowCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED)) 
                return;
            string message = string.Join(": ", 
                GetType().Name, nameof(OnUnityAdsShowComplete), _PlacementId, _ShowCompletionState);
            Dbg.Log(message);
            m_DoInvokeOnShown = true;
            Ready = false;
            LoadAd();
        }
    }
}