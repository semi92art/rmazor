using System;
using Games.Main_Mode.StageBlocks;
using UnityEngine;

namespace Games.Main_Mode.BlockObstacles
{
    [Flags]
    public enum BlockObstacleType
    {
        Simple,
        Static
    }
    
    public abstract class BlockObstacleBase : MonoBehaviour, IUpdateState
    {
        [SerializeField] protected new Collider2D collider;
        [SerializeField] protected Rigidbody2D rb;
        
        protected float Speed { get; set; }
        protected float RelativeRawPosition { get; private set; }
        protected int BlockIndex { get; private set; }
        protected int ObstacleIndex { get; private set; }

        private Func<float, Vector2> m_GetPosition;
        private Func<float, float> m_GetAngle;

        protected virtual void Init(BlockObstacleProps _Props)
        {
            BlockIndex = _Props.BlockIndex;
            ObstacleIndex = _Props.ObstacleIndex;
            RelativeRawPosition = _Props.RelativeRawPosition;
            m_GetPosition = _Props.GetPosition;
            m_GetAngle = _Props.GetAngle;
            InitColliderAndShape(_Props.Color);
            InitRigidbody();
        }

        private void InitRigidbody()
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 0f;
            rb.mass = 0f;
            rb.bodyType = RigidbodyType2D.Static;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        
        protected abstract void InitColliderAndShape(Color _Color);
        public virtual void UpdateState(float _DeltaTime)
        {
            RelativeRawPosition += _DeltaTime * Speed;
            transform.localPosition = m_GetPosition.Invoke(RelativeRawPosition);
            transform.localEulerAngles = Vector3.forward * m_GetAngle(RelativeRawPosition);
        }
    }

    public class BlockObstacleProps
    {
        public BlockObstacleType Type { get; set; }
        public int BlockIndex { get; set; }
        public int ObstacleIndex { get; set; }
        public float RelativeRawPosition { get; set; }
        public Color Color { get; set; }
        public float Speed { get; set; }
        public Func<float, Vector2> GetPosition { get; set; }
        public Func<float, float> GetAngle { get; set; }
    }
}