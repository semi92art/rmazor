using Extensions;
using Shapes;
using UnityEngine;

namespace Games.Main_Mode.StageBlocks
{
    public class StageBlockCircle : StageBlockBase
    {
        [SerializeField] private Disc zone;
        public float Radius => zone.Radius;
        
        public static StageBlockCircle Create(Transform _Parent, StageBlockProps _Props)
        {
            var go = new GameObject($"Block Stage {_Props.BlockIndex + 1}");
            go.SetParent(_Parent);
            go.transform.localPosition = Vector3.zero;
            var block = go.AddComponent<StageBlockCircle>();
            block.Init(_Props);
            return block;
        }

        public static float GetRadius(int _BlockIndex)
        {
            return MainModeConstants.BlockWidth * (_BlockIndex + 0.5f);
        }

        protected override void SetZone(int _BlockIndex, Color _ZoneColor, Color _BorderColor)
        {
            zone = gameObject.AddComponent<Disc>();
            zone.SortingOrder = MainModeConstants.Zone0RenderOrder - _BlockIndex;
            zone.ColorMode = Disc.DiscColorMode.Single;
            zone.Color = _ZoneColor;
            zone.Type = DiscType.Ring;
            zone.Radius = GetRadius(_BlockIndex);
        }
        
        protected override Vector2 GetObstaclePosition(float _RelativeRawPosition)
        {
            float angle = GetObstacleAngle(_RelativeRawPosition);
            float x = Mathf.Cos(angle) * Radius;
            float y = Mathf.Sin(angle) * Radius;
            return new Vector2(x, y);
        }
        
        protected override float GetObstacleAngle(float _RelativeRawPosition)
        {
            return 270f * Mathf.Deg2Rad - 2f * Mathf.PI * _RelativeRawPosition;
        }
    }
}