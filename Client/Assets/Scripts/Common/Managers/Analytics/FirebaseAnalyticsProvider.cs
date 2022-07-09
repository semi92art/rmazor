using System.Collections.Generic;
using System.Linq;
using Firebase;
using Firebase.Analytics;

namespace Common.Managers.Analytics
{
    public interface IFirebaseAnalyticsProvider : IAnalyticsProvider { }

    public class FirebaseAnalyticsProvider : AnalyticsProviderBase, IFirebaseAnalyticsProvider
    {
        #region nonpublic members

        protected override Dictionary<string, string> ValidIdsAndNamesTranslations => null;

        #endregion
        
        #region api
        
        public override void Init()
        {
            InitializeFirebase();
            base.Init();
        }

        #endregion

        #region nonpublic methods
        
        private static void InitializeFirebase()
        {
            if (CommonData.FirebaseApp != null)
            {
                InitializeAnalytics();
                Dbg.Log("Firebase initialized successfully");
                return;
            }
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(_Task =>
            {
                if (_Task.Result == DependencyStatus.Available)
                {
                    CommonData.FirebaseApp = FirebaseApp.DefaultInstance;
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
        
        protected override void SendAnalyticCore(
            string                      _AnalyticId, 
            IDictionary<string, object> _EventData = null)
        {
            if (CommonData.FirebaseApp == null)
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
    }
}