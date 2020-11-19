using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace PointsTapper
{
    public class RandomPositionGenerator
    {
        private readonly Bounds m_Bounds;
        private readonly Vector4 m_Margin; // left,right,top,bottom
        private readonly IEnumerable<ISpawnPool<PointItem>> m_Pools;
        
        public RandomPositionGenerator(IEnumerable<ISpawnPool<PointItem>> _Pools, Vector4 _Margin)
        {
            m_Pools = _Pools;
            m_Bounds = GameUtils.GetVisibleBounds();
            m_Margin = _Margin;
        }
        
        public Vector2 Next(float _Indent)
        {
            bool generated = false;
            Vector2 result = default;
            for (int i = 0; i < 10000; i++)
            {
                bool intersects = false;
                Vector2 pos = RandomPositionInMarginRect(_Indent);
                foreach (var pool in m_Pools)
                {
                    foreach (var point in pool)
                    {
                        if (!point.Activated)
                            continue;
                        var dscPos = point.transform.position;
                        if (!GeometryUtils.CirclesIntersect(
                            pos, _Indent, dscPos, point.Radius))
                            continue;
                        intersects = true;
                        break;
                    }
                }
                if (intersects) 
                    continue;

                generated = true;
                result = pos;
                break;
            }
        
            if (!generated)
                Debug.LogWarning("Disc was not generated because of not enough space");
            return result;
        }
        
        private Vector2 RandomPositionInMarginRect(float _Radius)
        {
            float leftMargin = m_Margin.x;
            float rightMargin = m_Margin.y;
            float topMargin = m_Margin.z;
            float bottomMargin = m_Margin.w;

            float xCenter = 0.5f * (rightMargin - leftMargin);
            float xDelta = Utility.RandomGen.NextFloatAlt() *
                           (m_Bounds.size.x - _Radius * 2f - 0.5f * (rightMargin + leftMargin));
            float x = xCenter + xDelta;
            float yCenter = 0.5f * (topMargin - bottomMargin);
            float yDelta = Utility.RandomGen.NextFloatAlt() *
                           (m_Bounds.size.y - _Radius * 2f - 0.5f * (topMargin + bottomMargin));
            float y = yCenter + yDelta;
            return new Vector2(x, y);
        }
    }
}