using Ticker;
using UnityEngine.Advertisements;
using Utils;

namespace Managers.Advertising
{
    public interface IUnityAdsRewardedAd : IUnityAdsAd { }
    
    public class UnityAdsRewardedAd : UnityAdsAdBase, IUnityAdsRewardedAd
    {
        public UnityAdsRewardedAd(IViewGameTicker _GameTicker) : base(_GameTicker) { }
        
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