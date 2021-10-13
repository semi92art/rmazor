using UnityEngine;
using UnityEngine.Events;

namespace Managers
{
    public interface IAnalyticsManager : IInit
    {
        void OnAnalytic(string _AnalyticCode);
    }
    
    public class AnalyticsManager : IAnalyticsManager
    {
        #region singleton
        
        private static AnalyticsManager _instance;
        public static AnalyticsManager Instance => _instance ?? (_instance = new AnalyticsManager());
    
        #endregion
    
        #region nonpublic members
    
        private static GameObject _analyticsObject;
        
        #endregion

        #region api
        
        public event UnityAction Initialized;

        public void Init()
        { 
            // TODO
           // AnalyticsEventTracker analyticsEventTracker = _analyticsObject.AddComponent<AnalyticsEventTracker>();
           // analyticsEventTracker.m_Trigger.lifecycleEvent.
           //  
           //  Dbg.Log("Analytics enabled");
           Initialized?.Invoke();
        }

        public void OnAnalytic(string _AnalyticCode)
        {
            // TODO
        }


        #endregion
    }
}
