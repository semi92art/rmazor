using RMAZOR.Models;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.Additional_Background;
using UnityEngine;

namespace RMAZOR
{
    public static class CommonDataRmazor
    {
        public const int    RetroModeCharacterLevelToUnlock = 5;
        public const string CharacterIdDefault              = "01";
        public const string ColorSetIdDefault               = "white";
        
        public const int CharacterLevelOnStart = 1;
        
        public static int[] LevelsInGroupArray = {3, 3, 3};

        public static ViewCameraEffectsCustomAnimator              CameraEffectsCustomAnimator;
        public static ViewMazeBackgroundTextureControllerRmazor    BackgroundTextureControllerRmazor;
        public static ViewMazeAdditionalBackgroundDrawerRmazorFull AdditionalBackgroundDrawer;

        public static LevelStageArgs LastLevelStageArgs;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetState()
        {
            CameraEffectsCustomAnimator       = null;
            BackgroundTextureControllerRmazor = null;
            AdditionalBackgroundDrawer        = null;
            LastLevelStageArgs                = null;
        }
    }
}