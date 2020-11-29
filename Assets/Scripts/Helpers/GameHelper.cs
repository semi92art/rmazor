using System.Collections.Generic;
using System.Linq;
using Extensions;
using Shapes;
using UnityEngine;
using Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Helpers
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
                Debug.LogWarning("Disc was not generated because of not enough space");
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
                .Where(_Kvp => !_Kvp.Value.IsAlive()))
                list.Remove(kvp);
        }

        private Vector2 RandomPositionInMarginRect()
        {
            var bounds = GameUtils.GetVisibleBounds();
            float x = CommonUtils.RandomGen.NextFloatAlt() * (bounds.size.x - horMargin);
            float y = bottomMargin * 0.5f - topMargin * 0.5f + CommonUtils.RandomGen.NextFloatAlt() *
                      (bounds.size.y - topMargin * 0.5f - bottomMargin * 0.5f);
            return new Vector2(x, y);
        }
    
        private void OnDrawGizmos()
        {
            var bounds = GameUtils.GetVisibleBounds();
            var topLeft = new Vector3(
                bounds.center.x - bounds.size.x + horMargin,
                bounds.center.y + bounds.size.y - topMargin);
            var topRight = new Vector3(
                bounds.center.x + bounds.size.x - horMargin,
                bounds.center.y + bounds.size.y - topMargin);
            var bottomLeft = new Vector3(
                bounds.center.x - bounds.size.x + horMargin,
                bounds.center.y - bounds.size.y + bottomMargin);
            var bottomRight = new Vector3(
                bounds.center.x + bounds.size.x - horMargin,
                bounds.center.y - bounds.size.y + bottomMargin);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(GameHelper))]
    public class GameHelperEditor : Editor
    {
        private GameHelper m_GameHelper;
        private float m_DiscNum = 5;
        private float m_Radius = 2;
        private float m_RadiusIndent = 1;

        private void OnEnable()
        {
            m_GameHelper = (GameHelper) target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var bounds = GameUtils.GetVisibleBounds();
            GUILayout.Label("Camera View Rect:");
            GUILayout.Label($"Width: {bounds.size.x * 2:f2}\tHeight: {bounds.size.y * 2:f2}");
            GUILayout.Label($"Width/2: {bounds.size.x:f2}\tHeight/2: {bounds.size.y:f2}");
            GUILayout.Label("Margin Rect:");
            GUILayout.Label($"Left: {-bounds.size.x + m_GameHelper.horMargin:f2}\t" +
                            $"Right: {bounds.size.x - m_GameHelper.horMargin:f2}");
            GUILayout.Label($"Top: {bounds.size.y - m_GameHelper.topMargin}\t\t" +
                            $"Bottom: {-bounds.size.y + m_GameHelper.bottomMargin}");
            EditorUtils.DrawUiLine(Color.gray);
        
        
            if (GUILayout.Button("Generate Disc"))
            {
                for (int i = 0; i < m_DiscNum; i++)
                    m_GameHelper.GenerateDiscWithRandomPosition(m_Radius, m_RadiusIndent);
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Cnt");
            m_DiscNum = EditorGUILayout.FloatField(m_DiscNum);
            GUILayout.Label("R");
            m_Radius = EditorGUILayout.FloatField(m_Radius);
            GUILayout.Label("R Indent");
            m_RadiusIndent = EditorGUILayout.FloatField(m_RadiusIndent);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Clear"))
                m_GameHelper.ClearDiscs();
            EditorUtils.DrawUiLine(Color.gray);
        }
    }

#endif
}