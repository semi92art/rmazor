using System.Collections.Generic;
using Common.Constants;
using Common.Helpers;

namespace Common.Managers.Analytics
{
    public interface IAnalyticsManager : IAnalyticsProvider { }
    
    public class AnalyticsManager : InitBase, IAnalyticsManager
    {
        #region inject

        private IRemotePropertiesCommon    RemoteProperties          { get; }
        private IUnityAnalyticsProvider    UnityAnalyticsProvider    { get; }
        private IFirebaseAnalyticsProvider FirebaseAnalyticsProvider { get; }

        private AnalyticsManager(
            IRemotePropertiesCommon    _RemoteProperties,
            IUnityAnalyticsProvider    _UnityAnalyticsProvider,
            IFirebaseAnalyticsProvider _FirebaseAnalyticsProvider)
        {
            RemoteProperties          = _RemoteProperties;
            UnityAnalyticsProvider    = _UnityAnalyticsProvider;
            FirebaseAnalyticsProvider = _FirebaseAnalyticsProvider;
        }
        
        #endregion

        #region api
        
        public override void Init()
        {
            if (Initialized)
                return;
            if (!RemoteProperties.DebugEnabled)
            {
                UnityAnalyticsProvider.Init();
                FirebaseAnalyticsProvider.Init();
            }
            base.Init();
        }
        
        public void SendAnalytic(string _AnalyticId, IDictionary<string, object> _EventData = null)
        {
            // if (RemoteProperties.DebugEnabled && _AnalyticId != AnalyticIds.TestAnalytic)
            //     return;
            UnityAnalyticsProvider.SendAnalytic(_AnalyticId, _EventData);
            FirebaseAnalyticsProvider.SendAnalytic(_AnalyticId, _EventData);
        }
        
        #endregion
    }

    public class AnalyticsManagerFake : InitBase, IAnalyticsManager
    {
        public void SendAnalytic(string _AnalyticId, IDictionary<string, object> _EventData = null) { }
    }
}