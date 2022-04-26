using UnityEngine;

namespace Common
{
    public class CommonData
    {
        public static bool   LoadNextLevelAutomatically;
        public static int    GameId;
        public static bool   DevelopmentBuild;
        public static bool   PausedByAdvertisingOrPurchasing = false;
        public static bool   Release             = false;
        public static bool   Testing             = false;
        public const  string SavedGameFileName   = "main_save";
        
        public static readonly Color CompanyLogoBackgroundColor = new Color(0.37f, 0.07f, 0.25f);
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ResetState()
        {
            LoadNextLevelAutomatically = false;
            GameId                     = 1;
            DevelopmentBuild           = false;
#if !UNITY_EDITOR && DEVELOPMENT_BUILD
            DevelopmentBuild = true;
#endif
        }
    }
}