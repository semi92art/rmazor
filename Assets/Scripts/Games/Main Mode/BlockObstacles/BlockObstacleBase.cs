using UnityEngine;

namespace Games.Main_Mode.BlockObstacles
{
    public interface IBlockObstacle
    {
        void OnCollide(Collider2D _C);
    }
    
    public abstract class BlockObstacleBase : MonoBehaviour, IBlockObstacle
    {
        [SerializeField] private new Collider2D collider;
        private int m_StageIndex;
        private float m_StartPosition;

        protected virtual void Init(int _StageIndex, Collider2D _Collider, float _StartPosition)
        {
            m_StageIndex = _StageIndex;
            collider = _Collider;
        }

        public abstract void OnCollide(Collider2D _C);
    }
}