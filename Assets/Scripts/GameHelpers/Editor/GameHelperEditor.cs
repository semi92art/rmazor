using UnityEditor;
using UnityEngine;
using Utils;
using Utils.Editor;

namespace GameHelpers.Editor
{
    [CustomEditor(typeof(GameHelper))]
    public class GameHelperEditor : UnityEditor.Editor
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
            EditorUtilsEx.DrawUiLine(Color.gray);


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
            EditorUtilsEx.DrawUiLine(Color.gray);
        }
    }
}