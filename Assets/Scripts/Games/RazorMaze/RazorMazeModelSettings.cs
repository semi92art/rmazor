using UnityEngine;

namespace Games.RazorMaze
{
    public class RazorMazeModelSettings : MonoBehaviour
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
        
        public float turretShootInterval;
        public float turretProjectileSpeed;
    }
}