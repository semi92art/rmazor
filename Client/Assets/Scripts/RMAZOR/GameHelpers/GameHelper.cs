using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Extensions;
using Common.Utils;
using Shapes;
using UnityEngine;
using Utils;

namespace GameHelpers
{
    public class GameHelper : MonoBehaviour
    {
        [Range(0, 10)] public float horMargin;
        [Range(0, 10)] public float topMargin;
        [Range(0, 10)] public float bottomMargin;

        private Dictionary<int, Disc> m_DiscDict = new Dictionary<int, Disc>();
    
        public void GenerateDiscWithRandomPosition(float _Radius, float _RadiusIndent)
        {
            ClearEmptyDiscs();
        
            bool generated = false;
            for (int i = 0; i < 10000; i++)
            {
                bool intersects = false;
                Vector2 pos = RandomPositionInMarginRect();
                foreach (var dsc in m_DiscDict)
                {
                    var dscPos = dsc.Value.transform.position;
                    if (!GeometryUtils.CirclesIntersect(
                        pos, _Radius + _RadiusIndent, dscPos, dsc.Value.Radius))
                        continue;
                    intersects = true;
                    break;
                }

                if (intersects) 
                    continue;
                GameObject go = new GameObject("Disc");
                var disc = go.AddComponent<Disc>();
                disc.RadiusSpace = ThicknessSpace.Meters;
                disc.Radius = _Radius;
                disc.transform.position = pos;
                m_DiscDict.Add(disc.GetInstanceID(), disc);
                generated = true;
                break;
            }
        
            if (!generated)
                Dbg.LogWarning("Disc was not generated because of not enough space");
        }

        public void ClearDiscs()
        {
            ClearEmptyDiscs();
            foreach (var kvp in m_DiscDict.ToList())
            {
                DestroyImmediate(kvp.Value.gameObject);
                m_DiscDict.Remove(kvp.Key);
            }
        }

        private void ClearEmptyDiscs()
        {
            var list = m_DiscDict.ToList();
            foreach (var kvp in list
                .Where(_Kvp => _Kvp.Value.IsNull()))
                list.Remove(kvp);
        }

        private Vector2 RandomPositionInMarginRect()
        {
            var bounds = GraphicUtils.GetVisibleBounds();
            float x = MathUtils.RandomGen.NextFloatAlt() * (bounds.max.x - horMargin);
            float y = bottomMargin * 0.5f - topMargin * 0.5f + MathUtils.RandomGen.NextFloatAlt() *
                      (bounds.max.y - topMargin * 0.5f - bottomMargin * 0.5f);
            return new Vector2(x, y);
        }
    
        private void OnDrawGizmos()
        {
            var bounds = GraphicUtils.GetVisibleBounds();
            var topLeft = new Vector3(
                bounds.min.x + horMargin,
                bounds.max.y - topMargin);
            var topRight = new Vector3(
                bounds.max.x - horMargin,
                bounds.max.y - topMargin);
            var bottomLeft = new Vector3(
                bounds.min.x + horMargin,
                bounds.min.y + bottomMargin);
            var bottomRight = new Vector3(
                bounds.max.x - horMargin,
                bounds.min.y + bottomMargin);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }
        
        public void GenerateEdges()
        {
            string name = "Edges";
            var go = GameObject.Find(name);
            EdgeCollider2D coll;
            if (go == null)
            {
                go = new GameObject("Edges");
                go.AddComponent<EdgeCollider2D>();
            }

            coll = go.GetComponent<EdgeCollider2D>();
            var bounds = GraphicUtils.GetVisibleBounds();
            var a = new Vector2(bounds.min.x, bounds.max.y);
            var b = bounds.min.XY();
            var c = new Vector2(bounds.max.x, bounds.min.y);
            var d = bounds.max.XY(); 
            coll.points = new[] { a, b, c, d, a};
        }
    }
}