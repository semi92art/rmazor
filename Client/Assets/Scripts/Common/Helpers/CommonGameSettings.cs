using Common.Managers.Advertising;
using UnityEngine;

namespace Common.Helpers
{
    [CreateAssetMenu(fileName = "game_settings", menuName = "Configs and Sets/Game Settings", order = 1)]
    public class CommonGameSettings : ScriptableObject
    {
        public EAdsProvider adsProvider;
        public float        adsLoadDelay;
        public bool         debugEnabled;
        public bool         testAds;
        public ELogLevel    logLevel;
        public float        admobRate;
        public float        unityAdsRate;
        public float        ironSourceRate;
        public int          showAdsEveryLevel;
        public int          firstLevelToShowAds;
        public bool         rewriteSettingsByRemoteConfigInEditor;
        public int          payToContinueMoneyCount;
        public bool         showRewardedInsteadOfInterstitialOnUnpause;
        public string       ironSourceAppKeyAndroid;
        public string       ironSourceAppKeyIos;
        public int          moneyItemCoast;
        public int          gameId;
    }
}