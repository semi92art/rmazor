using Entities;
using UnityEngine;

namespace Managers
{
    public class AnalyticsManager : GameObserver, ISingleton
    {
        #region singleton
        
        private static AnalyticsManager _instance;

        public static AnalyticsManager Instance => _instance ?? (_instance = new AnalyticsManager());
    
        #endregion
    
        #region nonpublic members
    
        private static GameObject _analyticsObject;
        
        #endregion

        #region api

        public void Init()
        {
           // AnalyticsEventTracker analyticsEventTracker = _analyticsObject.AddComponent<AnalyticsEventTracker>();
           // analyticsEventTracker.m_Trigger.lifecycleEvent.
           //  
           //  Debug.Log("Analytics enabled");
        }
        #endregion

        #region nonpubli methods
        
        protected override void OnNotify(object _Sender, string _NotifyMessage, params object[] _Args)
        {
            // TODO
        }
        
        #endregion
    }
}
