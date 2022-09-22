using System.Collections.Generic;

namespace Common.Managers.Analytics
{
    public interface IAnalyticsProvidersSet
    {
        List<IAnalyticsProvider> GetProviders();
    }
    
    public class AnalyticsProvidersSet : IAnalyticsProvidersSet
    {
        private    IUnityAnalyticsProvider    UnityAnalyticsProvider    { get; }
        private    IFirebaseAnalyticsProvider FirebaseAnalyticsProvider { get; }
#if APPODEAL_3
        [Zenject.Inject] private IAppodealAnalyticsProvider AppodealAnalyticsProvider { get; }
#endif

        private AnalyticsProvidersSet(
            IUnityAnalyticsProvider    _UnityAnalyticsProvider,
            IFirebaseAnalyticsProvider _FirebaseAnalyticsProvider)
        {
            UnityAnalyticsProvider    = _UnityAnalyticsProvider;
            FirebaseAnalyticsProvider = _FirebaseAnalyticsProvider;
        }

        public List<IAnalyticsProvider> GetProviders()
        {
            var providers = new List<IAnalyticsProvider>();
            providers.AddRange(new IAnalyticsProvider[]
            {
                UnityAnalyticsProvider, FirebaseAnalyticsProvider
            });
#if APPODEAL_3
            providers.Add(AppodealAnalyticsProvider);
#endif
            return providers;
        }
    }
}