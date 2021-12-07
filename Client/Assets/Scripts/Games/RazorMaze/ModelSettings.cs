using UnityEngine;

namespace Games.RazorMaze
{
    [CreateAssetMenu(fileName = "model_settings", menuName = "Configs and Sets/Model Settings", order = 1)]
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

        public float CharacterSpeed
        {
            get => characterSpeed;
            set => characterSpeed = value;
        }

        public float MovingItemsSpeed
        {
            get => movingItemsSpeed;
            set => movingItemsSpeed = value;
        }

        public float GravityBlockSpeed
        {
            get => gravityBlockSpeed;
            set => gravityBlockSpeed = value;
        }

        public float GravityTrapSpeed
        {
            get => gravityTrapSpeed;
            set => gravityTrapSpeed = value;
        }

        public float MovingItemsPause
        {
            get => movingItemsPause;
            set => movingItemsPause = value;
        }

        public float TrapPreReactTime
        {
            get => trapPreReactTime;
            set => trapPreReactTime = value;
        }

        public float TrapReactTime
        {
            get => trapReactTime;
            set => trapReactTime = value;
        }

        public float TrapAfterReactTime
        {
            get => trapAfterReactTime;
            set => trapAfterReactTime = value;
        }

        public float TrapIncreasingIdleTime
        {
            get => trapIncreasingIdleTime;
            set => trapIncreasingIdleTime = value;
        }

        public float TrapIncreasingIncreasedTime
        {
            get => trapIncreasingIncreasedTime;
            set => trapIncreasingIncreasedTime = value;
        }

        public float TurretPreShootInterval
        {
            get => turretPreShootInterval;
            set => turretPreShootInterval = value;
        }

        public float TurretShootInterval
        {
            get => turretShootInterval;
            set => turretShootInterval = value;
        }

        public float TurretProjectileSpeed
        {
            get => turretProjectileSpeed;
            set => turretProjectileSpeed = value;
        }

        public float ShredingerBlockProceedTime
        {
            get => shredingerBlockProceedTime;
            set => shredingerBlockProceedTime = value;
        }
    }
}