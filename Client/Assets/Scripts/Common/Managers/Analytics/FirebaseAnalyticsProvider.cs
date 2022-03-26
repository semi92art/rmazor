using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using Firebase;
using Firebase.Analytics;

namespace Common.Managers.Analytics
{
    public interface IFirebaseAnalyticsProvider : IAnalyticsProvider { }

    public class FirebaseAnalyticsProvider : InitBase, IFirebaseAnalyticsProvider
    {
        #region nonpublic members
        
        private FirebaseApp m_Instance;

        #endregion

        #region api
        
        public override void Init()
        {
            InitializeFirebase();
            base.Init();
        }
        
        public void SendAnalytic(
            string                      _AnalyticId, 
            IDictionary<string, object> _EventData = null)
        {
            if (m_Instance == null)
                return;
            if (_EventData == null)
            {
                FirebaseAnalytics.LogEvent(_AnalyticId);
                return;
            }
            var @params = _EventData.Select(_Kvp =>
            {
                (string key, var value) = _Kvp;
                Parameter p = value switch
                {
                    short shortVal   => new Parameter(key, shortVal),
                    int intVal       => new Parameter(key, intVal),
                    long longVal     => new Parameter(key, longVal),
                    float floatVal   => new Parameter(key, floatVal),
                    double doubleVal => new Parameter(key, doubleVal),
                    string stringVal => new Parameter(key, stringVal),
                    _                => null
                };
                return p;
            }).Where(_P => _P != null).ToArray();
            FirebaseAnalytics.LogEvent(_AnalyticId, @params);
        }

        #endregion

        #region nonpublic methods
        
        private void InitializeFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(_Task =>
            {
                if (_Task.Result == DependencyStatus.Available)
                {
                    m_Instance = FirebaseApp.DefaultInstance;
                    InitializeAnalytics();
                    Dbg.Log("Firebase initialized successfully");
                } 
                else
                    Dbg.LogError($"Could not resolve all Firebase dependencies: {_Task.Result}");
            });
        }
        
        private static void InitializeAnalytics()
        {
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            FirebaseAnalytics.SetUserProperty(
                FirebaseAnalytics.UserPropertySignUpMethod,
                "Google");
        }

        #endregion
    }
}