using Extensions;
using Shapes;
using UnityEngine;
using Utils;

namespace Games.Main_Mode.StageBlocks
{
    public class StageBlockSquare : StageBlockBase
    {
        [SerializeField] private Rectangle zone;
        [SerializeField] private Rectangle border;
        
        public static StageBlockSquare Create(Transform _Parent, StageBlockProps _Props)
        {
            var go = new GameObject($"Block Stage {_Props.BlockIndex + 1}");
            go.SetParent(_Parent);
            go.transform.localPosition = Vector3.zero;
            var block = go.AddComponent<StageBlockSquare>();
            block.Init(_Props);
            return block;
        }

        protected override void SetZone(int _BlockIndex, Color _ZoneColor, Color _BorderColor)
        {
            zone = gameObject.AddComponent<Rectangle>();
            zone.SortingOrder = MainModeConstants.Zone0RenderOrder - 2 * _BlockIndex;
            zone.Color = _ZoneColor;
            zone.Type = Rectangle.RectangleType.HardHollow;
            float zoneSize = MainModeConstants.BlockWidth * _BlockIndex * 2f;
            zone.Width = zone.Height = zoneSize;
            zone.Thickness = MainModeConstants.BlockWidth;
            
            var borderGo = new GameObject("Border");
            borderGo.SetParent(transform);
            borderGo.transform.localPosition = Vector3.zero;
            border = borderGo.AddComponent<Rectangle>();
            border.SortingOrder = MainModeConstants.Zone0RenderOrder - 2 * _BlockIndex + 1;
            border.Color = _BorderColor;
            float borderSize = MainModeConstants.BlockWidth * _BlockIndex * 2f;
            border.Width = border.Height = borderSize;
            border.Thickness = MainModeConstants.ZoneBorderThickness;
        }

        protected override Vector2 GetObstaclePosition(float _RelativeRawPosition)
        {
            float trajW = zone.Width - zone.Thickness * 0.5f;
            float trajH = trajW;
            float trajectoryPerimeterLength = trajW * 2f + trajH * 2f;
            float rawPos = MathUtils.Fraction(_RelativeRawPosition) * trajectoryPerimeterLength;
            
            if (rawPos < trajH)
                return new Vector2(-trajW * 0.5f, -trajH * 0.5f + rawPos);
            if (rawPos < trajH + trajW)
            {
                rawPos -= trajH;
                return new Vector2(-trajW * 0.5f + rawPos, trajH * 0.5f);
            }
            if (rawPos < trajH * 2f + trajW)
            {
                rawPos -= trajH + trajW;
                return new Vector2(trajW * 0.5f, trajH * 0.5f - rawPos);
            }

            rawPos -= trajH * 2f + trajW;
            return new Vector2(trajW * 0.5f - rawPos, -trajH * 0.5f);
        }
        
        protected override float GetObstacleAngle(float _RelativeRawPosition)
        {
            if (_RelativeRawPosition < 0.25f)
                return 90f;
            if (_RelativeRawPosition < 0.5f)
                return 0f;
            if (_RelativeRawPosition < 0.75f)
                return 270f;
            return 180f;
        }
    }
}