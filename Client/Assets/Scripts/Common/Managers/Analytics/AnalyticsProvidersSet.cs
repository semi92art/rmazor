using System.Collections.Generic;

namespace Common.Managers.Analytics
{
    public interface IAnalyticsProvidersSet
    {
        List<IAnalyticsProvider> GetProviders();
    }
    
    public class AnalyticsProvidersSet : IAnalyticsProvidersSet
    {
        private IUnityAnalyticsProvider    UnityAnalyticsProvider    { get; }
        private IFirebaseAnalyticsProvider FirebaseAnalyticsProvider { get; }
        private IAppodealAnalyticsProvider AppodealAnalyticsProvider { get; }

        private AnalyticsProvidersSet(
            IUnityAnalyticsProvider    _UnityAnalyticsProvider,
            IFirebaseAnalyticsProvider _FirebaseAnalyticsProvider, 
            IAppodealAnalyticsProvider _AppodealAnalyticsProvider)
        {
            UnityAnalyticsProvider    = _UnityAnalyticsProvider;
            FirebaseAnalyticsProvider = _FirebaseAnalyticsProvider;
            AppodealAnalyticsProvider = _AppodealAnalyticsProvider;
        }

        public List<IAnalyticsProvider> GetProviders()
        {
            return new List<IAnalyticsProvider>
            {
                UnityAnalyticsProvider, FirebaseAnalyticsProvider, AppodealAnalyticsProvider
            };
        }
    }
}