using System.Collections.Generic;
using UnityEngine.Analytics;
using UnityEngine.Events;

namespace Managers
{
    public interface IAnalyticsManager : IInit
    {
        void SendAnalytic(string _AnalyticId, IDictionary<string, object> _EventData = null);
    }
    
    public class AnalyticsManager : IAnalyticsManager
    {
        #region api
        
        public event UnityAction Initialized;

        public void Init()
        { 
            Initialized?.Invoke();
        }

        public void SendAnalytic(string _AnalyticId, IDictionary<string, object> _EventData = null)
        {
            Analytics.CustomEvent(_AnalyticId, _EventData);
        }
        
        #endregion
    }
}
