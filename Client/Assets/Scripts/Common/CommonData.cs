using UnityEngine;

namespace Common
{
    public class CommonData
    {
        public static bool   IsUnityEditor      = false;
        public static bool   IsDevelopmentBuild = false;
        public static bool   Release            = false;
        public static bool   Testing            = false;
        public const  string SavedGameFileName  = "main_save";
        
        public static readonly Color CompanyLogoBackgroundColor = new Color(0.37f, 0.07f, 0.25f);
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ResetState()
        {
#if UNITY_EDITOR
            IsUnityEditor = true;
#endif
#if DEVELOPMENT_BUILD
            IsDevelopmentBuild = true;
#endif
        }
    }
}