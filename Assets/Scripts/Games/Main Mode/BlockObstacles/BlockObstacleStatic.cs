using Extensions;
using Shapes;
using UnityEngine;

namespace Games.Main_Mode.BlockObstacles
{
    public class BlockObstacleStatic : BlockObstacleBase
    {
        [SerializeField] private Rectangle shape;
        
        public static BlockObstacleStatic Create(
            Transform _Parent,
            BlockObstacleProps _Props
        )
        {
            var go = new GameObject($"Obstacle {_Props.ObstacleIndex + 1}");
            go.SetParent(_Parent);
            var obstacle = go.AddComponent<BlockObstacleStatic>();
            obstacle.Init(_Props);
            return obstacle;
        }

        protected override void Init(BlockObstacleProps _Props)
        {
            Speed = 0;
            UpdateState(0);
            base.Init(_Props);
        }

        protected override void InitColliderAndShape(Color _Color)
        {
            float obstacleHeight = MainModeConstants.ObstacleSize;
            
            var coll = gameObject.AddComponent<BoxCollider2D>();
            coll.size = new Vector2(obstacleHeight * 0.5f, obstacleHeight);
            coll.isTrigger = true;
            collider = coll;

            shape = gameObject.AddComponent<Rectangle>();
            shape.SortingOrder = MainModeConstants.ObstacleRenderOrder;
            shape.Type = Rectangle.RectangleType.HardSolid;
            shape.Width = obstacleHeight * 0.5f;
            shape.Height = obstacleHeight;
            shape.Color = _Color;
        }
    }
}