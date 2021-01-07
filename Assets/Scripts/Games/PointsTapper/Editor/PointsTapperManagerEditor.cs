using UnityEditor;
using UnityEngine;

namespace Games.PointsTapper.Editor
{
    [CustomEditor(typeof(PointsTapperManager))]
    public class PointsTapperManagerEditor : UnityEditor.Editor
    {
        private PointsTapperManager m_Manager;
        private float m_Radius = 2f;

        private void OnEnable()
        {
            m_Manager = (PointsTapperManager) target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Normal"))
                m_Manager.GenerateItem(PointType.Default, m_Radius);
            if (GUILayout.Button("Bad"))
                m_Manager.GenerateItem(PointType.Bad, m_Radius);
            GUILayout.Label("Radius: ");
            m_Radius = EditorGUILayout.FloatField(m_Radius);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Do Instantiate"))
                m_Manager.DoInstantiate = true;
            if (GUILayout.Button("Do Not Instantiate"))
                m_Manager.DoInstantiate = false;
            GUILayout.EndHorizontal();
        }
    }
}