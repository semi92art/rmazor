using Entities;
using UI;
using UnityEngine;

namespace Managers
{
    public class AnalyticsManager : IGameObserver, IInit
    {
        #region singleton
        
        private static AnalyticsManager _instance;
        public static AnalyticsManager Instance => _instance ?? (_instance = new AnalyticsManager());
    
        #endregion
    
        #region nonpublic members
    
        private static GameObject _analyticsObject;
        
        #endregion

        #region api
        
        public event NoArgsHandler Initialized;

        public void Init()
        { 
            // TODO
           // AnalyticsEventTracker analyticsEventTracker = _analyticsObject.AddComponent<AnalyticsEventTracker>();
           // analyticsEventTracker.m_Trigger.lifecycleEvent.
           //  
           //  Dbg.Log("Analytics enabled");
           Initialized?.Invoke();
        }

        public void OnNotify( string _NotifyMessage, params object[] _Args)
        {
            // TODO
        }

        #endregion
    }
}
