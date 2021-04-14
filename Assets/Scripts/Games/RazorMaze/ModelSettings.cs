using UnityEngine;

namespace Games.RazorMaze
{
    [CreateAssetMenu(fileName = "model_settings", menuName = "Model Settings", order = 1)]
    public class ModelSettings : ScriptableObject
    {
        public float characterSpeed;
        public float mazeRotateSpeed;
        
        public float movingItemsSpeed;
        public float movingItemsPause;
        
        public float trapPreReactTime;
        public float trapReactTime;
        public float trapAfterReactTime;

        public float trapIncreasingIdleTime;
        public float trapIncreasingIncreasedTime;

        public float turretPreShootInterval;
        public float turretShootInterval;
        public float turretProjectileSpeed;

        public float shredingerBlockProceedTime;
    }
}