using UnityEngine.Advertisements;
using Utils;

namespace Managers.Advertising
{
    public interface IUnityAdsRewardedAd : IUnityAdsAd { }
    
    public class UnityAdsRewardedAd : UnityAdsAdBase, IUnityAdsRewardedAd
    {
        public override void OnUnityAdsShowComplete(string _PlacementId, UnityAdsShowCompletionState _ShowCompletionState)
        {
            if (!_PlacementId.Equals(m_UnitId)
                || !_ShowCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED)) 
                return;
            Dbg.Log("Unity Ads Rewarded Ad Completed");
            m_OnShown?.Invoke();
            Ready = false;
            Advertisement.Load(m_UnitId, this);
        }
    }
}