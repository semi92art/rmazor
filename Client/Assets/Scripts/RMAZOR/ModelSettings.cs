using UnityEngine;

namespace RMAZOR
{
    [CreateAssetMenu(fileName = "model_settings", menuName = "Configs and Sets/Model Settings", order = 1)]
    public class ModelSettings : ScriptableObject
    {
        public float characterSpeed;
        public float movingItemsSpeed;
        public float gravityBlockSpeed;
        public float gravityTrapSpeed;
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
        public float hammerShotPause;
        public float spearAppearPause;
        public int   spearShotsPerUnit;
        public float spearShotPause;
    }
}