using Common.Utils;
using RMAZOR.Helpers;
using UnityEditor;
using UnityEngine;
using Utils;

namespace GameHelpers.Editor
{
    [CustomEditor(typeof(GameHelper))]
    public class GameHelperEditor : UnityEditor.Editor
    {
        private GameHelper m_Target;
        private float m_DiscNum = 5;
        private float m_Radius = 2;
        private float m_RadiusIndent = 1;

        private void OnEnable()
        {
            m_Target = (GameHelper) target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var bounds = GraphicUtils.GetVisibleBounds();
            GUILayout.Label("Camera View Rect:");
            GUILayout.Label($"Width: {bounds.size.x:f2}\tHeight: {bounds.size.y:f2}");
            GUILayout.Label($"Width/2: {bounds.size.x * 0.5f:f2}\tHeight/2: {bounds.size.y * 0.5f:f2}");
            GUILayout.Label("Margin Rect:");
            GUILayout.Label($"Left: {bounds.min.x + m_Target.horMargin:f2}\t" +
                            $"Right: {bounds.max.x - m_Target.horMargin:f2}");
            GUILayout.Label($"Top: {bounds.max.y - m_Target.topMargin}\t\t" +
                            $"Bottom: {bounds.min.y + m_Target.bottomMargin}");
            EditorUtilsEx.HorizontalLine(Color.gray);


            if (GUILayout.Button("Generate Disc"))
            {
                for (int i = 0; i < m_DiscNum; i++)
                    m_Target.GenerateDiscWithRandomPosition(m_Radius, m_RadiusIndent);
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
                m_Target.ClearDiscs();
            EditorUtilsEx.HorizontalLine(Color.gray);
            
            EditorUtilsEx.GuiButtonAction("Generate Edges", m_Target.GenerateEdges);
        }
    }
}