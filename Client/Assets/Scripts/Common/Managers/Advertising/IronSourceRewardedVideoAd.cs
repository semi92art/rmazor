using UnityEngine.Events;

namespace Common.Managers.Advertising
{
    public interface IIronSourceRewardedVideoAd  : IAdBase { }
    
    public class IronSourceRewardedVideoAd : IIronSourceRewardedVideoAd
    {
        #region nonpublic members
        
        private string      m_RewardedInstanceId = "0";
        private UnityAction m_OnShown;

        #endregion

        #region api
        
        public bool Ready => IronSource.Agent.isRewardedVideoAvailable();
        
        public void Init(string _AppId, string _UnitId)
        {
            InitEvents();
            LoadAd();
        }

        public void ShowAd(UnityAction _OnShown)
        {
            m_OnShown = _OnShown;
            if (IronSource.Agent.isRewardedVideoAvailable()) 
            {
                IronSource.Agent.showRewardedVideo ();
            } 
            else
            {
                Dbg.LogWarning("IronSource.Agent.isRewardedVideoAvailable - False");
                LoadAd();
            }
        }

        public void LoadAd()
        {
            IronSource.Agent.loadRewardedVideo();
        }

        #endregion

        #region nonpublic methods
        
        private void InitEvents()
        {
            //Add Rewarded Video Events
            IronSourceEvents.onRewardedVideoAdOpenedEvent            += RewardedVideoAdOpenedEvent;
            IronSourceEvents.onRewardedVideoAdClosedEvent            += RewardedVideoAdClosedEvent; 
            IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardedVideoAvailabilityChangedEvent;
            IronSourceEvents.onRewardedVideoAdStartedEvent           += RewardedVideoAdStartedEvent;
            IronSourceEvents.onRewardedVideoAdEndedEvent             += RewardedVideoAdEndedEvent;
            IronSourceEvents.onRewardedVideoAdRewardedEvent          += RewardedVideoAdRewardedEvent; 
            IronSourceEvents.onRewardedVideoAdShowFailedEvent        += RewardedVideoAdShowFailedEvent; 
            IronSourceEvents.onRewardedVideoAdClickedEvent           += RewardedVideoAdClickedEvent;
            //Add Rewarded Video DemandOnly Events
            IronSourceEvents.onRewardedVideoAdOpenedDemandOnlyEvent     += RewardedVideoAdOpenedDemandOnlyEvent;
            IronSourceEvents.onRewardedVideoAdClosedDemandOnlyEvent     += RewardedVideoAdClosedDemandOnlyEvent;
            IronSourceEvents.onRewardedVideoAdLoadedDemandOnlyEvent     += RewardedVideoAdLoadedDemandOnlyEvent;
            IronSourceEvents.onRewardedVideoAdRewardedDemandOnlyEvent   += RewardedVideoAdRewardedDemandOnlyEvent; 
            IronSourceEvents.onRewardedVideoAdShowFailedDemandOnlyEvent += RewardedVideoAdShowFailedDemandOnlyEvent; 
            IronSourceEvents.onRewardedVideoAdClickedDemandOnlyEvent    += RewardedVideoAdClickedDemandOnlyEvent;
            IronSourceEvents.onRewardedVideoAdLoadFailedDemandOnlyEvent += RewardedVideoAdLoadFailedDemandOnlyEvent;
        }

        #endregion

        #region event methods

        private static void RewardedVideoAdOpenedEvent()
        {
            Dbg.Log(nameof(RewardedVideoAdOpenedEvent));
        }
        
        private static void RewardedVideoAdClosedEvent()
        {
            Dbg.Log(nameof(RewardedVideoAdClosedEvent));
        }
        
        private static void RewardedVideoAvailabilityChangedEvent(bool _CanShowAd)
        {
            Dbg.Log(nameof(RewardedVideoAvailabilityChangedEvent) 
                    + " canShowAd: " + _CanShowAd);
        }
        
        private static void RewardedVideoAdStartedEvent()
        {
            Dbg.Log(nameof(RewardedVideoAdStartedEvent));
        }
        
        private static void RewardedVideoAdEndedEvent()
        {
            Dbg.Log(nameof(RewardedVideoAdEndedEvent));
        }
        
        private void RewardedVideoAdRewardedEvent(IronSourcePlacement _Placement)
        {
            Dbg.Log(nameof(RewardedVideoAdRewardedEvent) + " placement: " + _Placement);
            m_OnShown?.Invoke();
            LoadAd();
        }
        
        private static void RewardedVideoAdShowFailedEvent(IronSourceError _Error)
        {
            Dbg.Log(nameof(RewardedVideoAdShowFailedEvent) + " error: " + _Error);
        }
        
        private static void RewardedVideoAdClickedEvent(IronSourcePlacement _Placement)
        {
            Dbg.Log(nameof(RewardedVideoAdClickedEvent) 
                    + " placement: " + _Placement);
        }
        
        private static void RewardedVideoAdOpenedDemandOnlyEvent(string _InstanceId)
        {
            Dbg.Log(nameof(RewardedVideoAdOpenedDemandOnlyEvent)
                    + " instance id: " + _InstanceId);
        }
        
        private static void RewardedVideoAdClosedDemandOnlyEvent(string _InstanceId)
        {
            Dbg.Log(nameof(RewardedVideoAdClosedDemandOnlyEvent) 
                    + " instance id: " + _InstanceId);
        }
        
        private static void RewardedVideoAdLoadedDemandOnlyEvent(string _InstanceId)
        {
            Dbg.Log(nameof(RewardedVideoAdLoadedDemandOnlyEvent)
                    + " instance id: " + _InstanceId);
        }
        
        private void RewardedVideoAdRewardedDemandOnlyEvent(string _InstanceId)
        {
            Dbg.Log(nameof(RewardedVideoAdRewardedDemandOnlyEvent)
                    + " instance id: " + _InstanceId);
        }
        
        private static void RewardedVideoAdShowFailedDemandOnlyEvent(string _InstanceId, IronSourceError _Error)
        {
            Dbg.Log(nameof(RewardedVideoAdShowFailedDemandOnlyEvent) 
                    + " instance id: " + _InstanceId
                    + ", error: " + _Error);
        }
        
        private static void RewardedVideoAdClickedDemandOnlyEvent(string _InstanceId)
        {
            Dbg.Log(nameof(RewardedVideoAdClickedDemandOnlyEvent) 
                    + " instance id: " + _InstanceId);
        }

        private static void RewardedVideoAdLoadFailedDemandOnlyEvent(string _InstanceId, IronSourceError _Error)
        {
            Dbg.Log(nameof(RewardedVideoAdLoadFailedDemandOnlyEvent) 
                    + " instance id: " + _InstanceId
                    + ", error: " + _Error);
        }
        
        #endregion
    }
}