using Extensions;
using Shapes;
using UnityEngine;

namespace Games.Main_Mode.BlockObstacles
{
    public class BlockObstacleSimple : BlockObstacleBase
    {
        [SerializeField] private Disc shape;
        
        public static BlockObstacleSimple Create(
            Transform _Parent,
            BlockObstacleProps _Props
            )
        {
            var go = new GameObject($"Obstacle {_Props.ObstacleIndex + 1}");
            go.SetParent(_Parent);
            var obstacle = go.AddComponent<BlockObstacleSimple>();
            obstacle.Init(_Props);
            return obstacle;
        }

        protected override void Init(BlockObstacleProps _Props)
        {
            Speed = _Props.Speed;
            base.Init(_Props);
        }
        
        protected override void InitColliderAndShape(Color _Color)
        {
            float shapeRadius = MainModeConstants.ObstacleSize;
            
            var coll = gameObject.AddComponent<CircleCollider2D>();
            coll.radius = shapeRadius;
            coll.isTrigger = true;
            collider = coll;

            shape = gameObject.AddComponent<Disc>();
            shape.SortingOrder = MainModeConstants.ObstacleRenderOrder;
            shape.Type = DiscType.Disc;
            shape.Radius = shapeRadius;
            shape.ColorMode = Disc.DiscColorMode.Single;
            shape.Color = _Color;
        }
    }
}