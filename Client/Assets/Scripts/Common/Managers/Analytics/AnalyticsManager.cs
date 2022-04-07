using System.Collections.Generic;
using Common.Helpers;

namespace Common.Managers.Analytics
{
    public interface IAnalyticsManager : IAnalyticsProvider { }
    
    public class AnalyticsManager : InitBase, IAnalyticsManager
    {
        #region inject

        private CommonGameSettings         GameSettings              { get; }
        private IUnityAnalyticsProvider    UnityAnalyticsProvider    { get; }
        private IFirebaseAnalyticsProvider FirebaseAnalyticsProvider { get; }

        public AnalyticsManager(
            CommonGameSettings         _GameSettings,
            IUnityAnalyticsProvider    _UnityAnalyticsProvider,
            IFirebaseAnalyticsProvider _FirebaseAnalyticsProvider)
        {
            GameSettings = _GameSettings;
            UnityAnalyticsProvider    = _UnityAnalyticsProvider;
            FirebaseAnalyticsProvider = _FirebaseAnalyticsProvider;
        }
        
        #endregion

        #region api
        
        public override void Init()
        {
            if (!GameSettings.debugEnabled)
            {
                UnityAnalyticsProvider.Init();
                FirebaseAnalyticsProvider.Init();
            }
            base.Init();
        }
        
        public void SendAnalytic(string _AnalyticId, IDictionary<string, object> _EventData = null)
        {
            if (GameSettings.debugEnabled)
                return;
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