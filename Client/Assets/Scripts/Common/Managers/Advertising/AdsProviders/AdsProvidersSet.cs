using System.Collections.Generic;

namespace Common.Managers.Advertising.AdsProviders
{
    public class AdsProvidersSet : IAdsProvidersSet
    {
#if ADMOB_API
        [Zenject.Inject] private IAdMobAdsProvider AdMobAdsProvider { get; }
#endif
#if UNITY_ADS_API
        [Zenject.Inject] private IUnityAdsProvider UnityAdsProvider { get; }
#endif
#if APPODEAL_3
        [Zenject.Inject] private IAppodealAdsProvider AppodealAdsProvider { get; }
#endif
#if YANDEX_GAMES
        private IYandexGamesAdsProvider YandexGamesAdsProvider { get; }
#endif

        public AdsProvidersSet(IYandexGamesAdsProvider _YandexGamesAdsProvider)
        {
            YandexGamesAdsProvider = _YandexGamesAdsProvider;
        }

        public List<IAdsProvider> GetProviders()
        {
            return new List<IAdsProvider>
            {
#if ADMOB_API
                AdMobAdsProvider,
#endif
#if UNITY_ADS_API
                UnityAdsProvider;
#endif
#if APPODEAL_3
                AppodealAdsProvider,
#endif
#if YANDEX_GAMES
                YandexGamesAdsProvider,
#endif
            };
        }
    }
}