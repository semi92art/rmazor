using System.Collections.Generic;
using Common.Helpers;

namespace Common.Managers.Analytics
{
    public interface IAnalyticsManager : IAnalyticsProvider { }
    
    public class AnalyticsManager : InitBase, IAnalyticsManager
    {
        #region inject

        private IUnityAnalyticsProvider    UnityAnalyticsProvider    { get; }
        private IFirebaseAnalyticsProvider FirebaseAnalyticsProvider { get; }

        public AnalyticsManager(
            IUnityAnalyticsProvider    _UnityAnalyticsProvider,
            IFirebaseAnalyticsProvider _FirebaseAnalyticsProvider)
        {
            UnityAnalyticsProvider    = _UnityAnalyticsProvider;
            FirebaseAnalyticsProvider = _FirebaseAnalyticsProvider;
        }
        
        #endregion

        #region api
        
        public override void Init()
        {
            UnityAnalyticsProvider.Init();
            FirebaseAnalyticsProvider.Init();
            base.Init();
        }
        
        public void SendAnalytic(string _AnalyticId, IDictionary<string, object> _EventData = null)
        {
            UnityAnalyticsProvider.SendAnalytic(_AnalyticId, _EventData);
            FirebaseAnalyticsProvider.SendAnalytic(_AnalyticId, _EventData);
        }
        
        #endregion
    }
}