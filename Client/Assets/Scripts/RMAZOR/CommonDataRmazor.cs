using RMAZOR.Views.Common;
using RMAZOR.Views.Common.Additional_Background;
using UnityEngine;

namespace RMAZOR
{
    public static class CommonDataRmazor
    {
        public static int[] LevelsInGroupArray = {3, 3, 3};
        
        public static ViewCameraEffectsCustomAnimator              CameraEffectsCustomAnimator;
        public static ViewMazeBackgroundTextureController          BackgroundTextureController;
        public static ViewMazeAdditionalBackgroundDrawerRmazorFull AdditionalBackgroundDrawer;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetState()
        {
            CameraEffectsCustomAnimator = null;
            BackgroundTextureController = null;
            AdditionalBackgroundDrawer = null;
        }
    }
}