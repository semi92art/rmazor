using Common.Helpers;
using mazing.common.Runtime;
using mazing.common.Runtime.Ticker;
using UnityEngine.Events;

namespace Common.Managers.Advertising.AdBlocks
{
    public interface IRewardedAdBase : IAdBase
    {
        void ShowAd(
            UnityAction _OnShown,
            UnityAction _OnClicked,
            UnityAction _OnReward,
            UnityAction _OnClosed,
            UnityAction _OnFailedToShow);
    }
    
    public abstract class RewardedAdBase : AdBase, IRewardedAdBase
    {
        #region nonpublic members

        protected        UnityAction OnReward;
        private volatile bool        m_DoInvokeOnReward;
        
        #endregion

        #region inject
        
        protected RewardedAdBase(
            GlobalGameSettings _GlobalGameSettings,
            ICommonTicker      _CommonTicker) 
            : base(_GlobalGameSettings, _CommonTicker) { }

        #endregion

        #region api
        
        public abstract void ShowAd(
            UnityAction _OnShown, 
            UnityAction _OnClicked, 
            UnityAction _OnReward,
            UnityAction _OnClosed,
            UnityAction _OnFailedToShow);

        public override void UpdateTick()
        {
            ProceedShownAction();
            ProceedClickedAction();
            ProceedRewardAction();
            ProceedClosedAction();
            ProceedFailedToShownAction();
            ProceedLoadAdOnDelay();
        }

        #endregion

        #region nonpublic methods
        
        private void ProceedRewardAction()
        {
            if (!m_DoInvokeOnReward) 
                return;
            Dbg.Log("Ad reward action");
            OnReward?.Invoke();
            OnReward = null;
            m_DoInvokeOnReward = false;
        }

        protected void OnAdRewardGot()
        {
            Dbg.Log($"{AdSource}: {AdType} reward got");
            m_DoInvokeOnReward = true;
        }

        #endregion
    }
}