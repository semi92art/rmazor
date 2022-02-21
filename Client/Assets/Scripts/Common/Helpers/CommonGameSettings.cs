using Common.Managers.Advertising;
using UnityEngine;

namespace Common.Helpers
{
    [CreateAssetMenu(fileName = "game_settings", menuName = "Configs and Sets/Game Settings", order = 1)]
    public class CommonGameSettings : ScriptableObject
    {
        [SerializeField] private EAdsProvider adsProvider;
        [SerializeField] private float        adsLoadDelay;
        [SerializeField] private bool         debugEnabled;
        public                   bool         testAds;
        [SerializeField] private ELogLevel    logLevel;
        public                   float        admobRate;
        public                   float        unityAdsRate;
        public                   int          showAdsEveryLevel;
        public                   int          firstLevelToShowAds;
        
        public EAdsProvider AdsProvider
        {
            get => adsProvider;
            set => adsProvider = value;
        }

        public float AdsLoadDelay
        {
            get => adsLoadDelay;
            set => adsLoadDelay = value;
        }

        public bool      DebugEnabled
        {
            get => debugEnabled;
            set => debugEnabled = value;
        }
        
        public ELogLevel LogLevel     => logLevel;
    }
}