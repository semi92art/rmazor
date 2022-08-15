using Common.Helpers;
using Common.Ticker;
using UnityEngine.Events;

namespace Common.Managers.Advertising.AdBlocks
{
    public interface IRewardedAdBase : IAdBase
    {
        void ShowAd(UnityAction _OnShown, UnityAction _OnClicked, UnityAction _OnReward);
    }
    
    public abstract class RewardedAdBase : AdBase, IRewardedAdBase
    {
        protected UnityAction OnReward;
        
        protected volatile bool DoInvokeOnReward;
        
        protected RewardedAdBase(
            GlobalGameSettings _GlobalGameSettings,
            ICommonTicker      _CommonTicker) 
            : base(_GlobalGameSettings, _CommonTicker) { }

        public abstract void ShowAd(UnityAction _OnShown, UnityAction _OnClicked, UnityAction _OnReward);

        public override void UpdateTick()
        {
            if (DoInvokeOnReward)
            {
                Dbg.Log("Ad reward action");
                OnReward?.Invoke();
                OnReward = null;
                DoInvokeOnReward = false;
            }
            base.UpdateTick();
        }

        protected virtual void OnAdRewardGot()
        {
            Dbg.Log($"{AdSource}: {AdType} reward got");
            DoInvokeOnReward = true;
        }
    }
}