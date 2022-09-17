using System.Collections.Generic;

namespace Common.Managers.Analytics
{
    public interface IAnalyticsProvidersSet
    {
        List<IAnalyticsProvider> GetProviders();
    }
    
    public class AnalyticsProvidersSet : IAnalyticsProvidersSet
    {
        [Zenject.Inject] IUnityAnalyticsProvider UnityAnalyticsProvider { get; }
#if FIREBASE
        [Zenject.Inject] private IFirebaseAnalyticsProvider FirebaseAnalyticsProvider { get; }
#endif
#if APPODEAL_3
        [Zenject.Inject] private IAppodealAnalyticsProvider AppodealAnalyticsProvider { get; }
#endif
        
        public List<IAnalyticsProvider> GetProviders()
        {
            var providers = new List<IAnalyticsProvider> {UnityAnalyticsProvider};
#if FIREBASE
            providers.Add(FirebaseAnalyticsProvider);
#endif
#if APPODEAL_3
            providers.Add(AppodealAnalyticsProvider);
#endif
            return providers;
        }
    }
}