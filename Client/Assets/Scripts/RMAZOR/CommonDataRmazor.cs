using Common.Entities;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.Additional_Background;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR
{
    public static class CommonDataRmazor
    {
        private static V2Int _lastMazeSize;
        
        public static V2Int LastMazeSize
        {
            get => _lastMazeSize;
            set
            {
                _lastMazeSize = value;
                LastMazeSizeChanged?.Invoke(value);
            }
        }

        public static event UnityAction<V2Int> LastMazeSizeChanged;
        
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
            _lastMazeSize = default;
            LastMazeSizeChanged = null;
        }
    }
}