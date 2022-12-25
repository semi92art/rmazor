using UnityEngine;

namespace Common
{
    public class MazorCommonData
    {
        public const string SavedGameFileName = "main_save";
        
        public static bool Release = false;
        public static bool Testing = false;

#if FIREBASE
        public static Firebase.FirebaseApp FirebaseApp;
#endif

        public static readonly Color CompanyLogoBackgroundColor = new Color(0.06f, 0f, 0.03f);
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void ResetState()
        {
#if FIREBASE
            FirebaseApp = null;
#endif
        }
    }
}