using Common.Helpers;
using UnityEngine.Events;

namespace Common.Managers.Advertising
{
    public interface IIronSourceInterstitialAd : IAdBase, IInit { }
    
    public class IronSourceInterstitialAd : InitBase, IIronSourceInterstitialAd
    {
        #region nonpublic members
        
        private string      m_RewardedInstanceId = "0";
        private UnityAction m_OnShown;
        
        #endregion
        
        #region api
        
        public bool Ready => IronSource.Agent.isInterstitialReady();
        
        public void Init(string _UnitId)
        {
            InitEvents();
            LoadAd();
        }

        public void ShowAd(UnityAction _OnShown)
        {
            m_OnShown = _OnShown;
            if (IronSource.Agent.isInterstitialReady()) 
            {
                IronSource.Agent.showInterstitial();
            } 
            else
            {
                Dbg.LogWarning("IronSource.Agent.isInterstitialReady - False");
                LoadAd();
            }
        }

        public void LoadAd()
        {
            IronSource.Agent.loadInterstitial();
        }

        #endregion

        #region nonpublic methods
        
        private void InitEvents()
        {
            // Add Interstitial Events
            IronSourceEvents.onInterstitialAdReadyEvent         += InterstitialAdReadyEvent;
            IronSourceEvents.onInterstitialAdLoadFailedEvent    += InterstitialAdLoadFailedEvent;		
            IronSourceEvents.onInterstitialAdShowSucceededEvent += InterstitialAdShowSucceededEvent; 
            IronSourceEvents.onInterstitialAdShowFailedEvent    += InterstitialAdShowFailedEvent; 
            IronSourceEvents.onInterstitialAdClickedEvent       += InterstitialAdClickedEvent;
            IronSourceEvents.onInterstitialAdOpenedEvent        += InterstitialAdOpenedEvent;
            IronSourceEvents.onInterstitialAdClosedEvent        += InterstitialAdClosedEvent;
            // Add Interstitial DemandOnly Events
            IronSourceEvents.onInterstitialAdReadyDemandOnlyEvent      += InterstitialAdReadyDemandOnlyEvent;
            IronSourceEvents.onInterstitialAdLoadFailedDemandOnlyEvent += InterstitialAdLoadFailedDemandOnlyEvent;		
            IronSourceEvents.onInterstitialAdShowFailedDemandOnlyEvent += InterstitialAdShowFailedDemandOnlyEvent; 
            IronSourceEvents.onInterstitialAdClickedDemandOnlyEvent    += InterstitialAdClickedDemandOnlyEvent;
            IronSourceEvents.onInterstitialAdOpenedDemandOnlyEvent     += InterstitialAdOpenedDemandOnlyEvent;
            IronSourceEvents.onInterstitialAdClosedDemandOnlyEvent     += InterstitialAdClosedDemandOnlyEvent;
        }


        #endregion

        #region event methods
        
        private void InterstitialAdReadyEvent()
        {
            Dbg.Log(nameof(InterstitialAdReadyEvent));
        }

        private void InterstitialAdLoadFailedEvent(IronSourceError _Error)
        {
            Dbg.Log(nameof(InterstitialAdLoadFailedEvent) + " error: " + _Error);
        }

        private void InterstitialAdShowSucceededEvent()
        {
            Dbg.Log(nameof(InterstitialAdShowSucceededEvent));
        }

        private void InterstitialAdShowFailedEvent(IronSourceError _Error)
        {
            Dbg.Log(nameof(InterstitialAdShowFailedEvent) + " error: " + _Error);
        }

        private void InterstitialAdClickedEvent()
        {
            Dbg.Log(nameof(InterstitialAdClickedEvent));
        }

        private void InterstitialAdOpenedEvent()
        {
            Dbg.Log(nameof(InterstitialAdOpenedEvent));
        }

        private void InterstitialAdClosedEvent()
        {
            Dbg.Log(nameof(InterstitialAdClosedEvent));
            m_OnShown?.Invoke();
            LoadAd();
        }

        private void InterstitialAdReadyDemandOnlyEvent(string _InstanceId)
        {
            Dbg.Log(nameof(InterstitialAdReadyDemandOnlyEvent)
                    + " instance id: " + _InstanceId);
        }

        private void InterstitialAdLoadFailedDemandOnlyEvent(string _InstanceId, IronSourceError _Error)
        {
            Dbg.Log(nameof(InterstitialAdLoadFailedDemandOnlyEvent) 
                    + " instance id: " + _InstanceId
                    + ", error: " + _Error);
        }

        private void InterstitialAdShowFailedDemandOnlyEvent(string _InstanceId, IronSourceError _Error)
        {
            Dbg.Log(nameof(InterstitialAdShowFailedDemandOnlyEvent) 
                    + " instance id: " + _InstanceId
                    + ", error: " + _Error);
        }

        private void InterstitialAdClickedDemandOnlyEvent(string _InstanceId)
        {
            Dbg.Log(nameof(InterstitialAdClickedDemandOnlyEvent)
                    + " instance id: " + _InstanceId);
        }

        private void InterstitialAdOpenedDemandOnlyEvent(string _InstanceId)
        {
            Dbg.Log(nameof(InterstitialAdOpenedDemandOnlyEvent)
                    + " instance id: " + _InstanceId);
        }

        private void InterstitialAdClosedDemandOnlyEvent(string _InstanceId)
        {
            Dbg.Log(nameof(InterstitialAdClosedDemandOnlyEvent)
                    + " instance id: " + _InstanceId);
        }

        #endregion
    }
}