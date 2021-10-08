using UnityEngine;

namespace Games.RazorMaze
{
    [CreateAssetMenu(fileName = "model_settings", menuName = "Model Settings", order = 1)]
    public class ModelSettings : ScriptableObject
    {
        [SerializeField] private float characterSpeed;
        [SerializeField] private float movingItemsSpeed;
        [SerializeField] private float gravityBlockSpeed;
        [SerializeField] private float gravityTrapSpeed;
        [SerializeField] private float movingItemsPause;
        [SerializeField] private float trapPreReactTime;
        [SerializeField] private float trapReactTime;
        [SerializeField] private float trapAfterReactTime;
        [SerializeField] private float trapIncreasingIdleTime;
        [SerializeField] private float trapIncreasingIncreasedTime;
        [SerializeField] private float turretPreShootInterval;
        [SerializeField] private float turretShootInterval;
        [SerializeField] private float turretProjectileSpeed;
        [SerializeField] private float shredingerBlockProceedTime;

        public float CharacterSpeed => characterSpeed;
        public float MovingItemsSpeed => movingItemsSpeed;
        public float GravityBlockSpeed => gravityBlockSpeed;
        public float GravityTrapSpeed => gravityTrapSpeed;
        public float MovingItemsPause => movingItemsPause;
        public float TrapPreReactTime => trapPreReactTime;
        public float TrapReactTime => trapReactTime;
        public float TrapAfterReactTime => trapAfterReactTime;
        public float TrapIncreasingIdleTime => trapIncreasingIdleTime;
        public float TrapIncreasingIncreasedTime => trapIncreasingIncreasedTime;
        public float TurretPreShootInterval => turretPreShootInterval;
        public float TurretShootInterval => turretShootInterval;
        public float TurretProjectileSpeed => turretProjectileSpeed;
        public float ShredingerBlockProceedTime => shredingerBlockProceedTime;
    }
}