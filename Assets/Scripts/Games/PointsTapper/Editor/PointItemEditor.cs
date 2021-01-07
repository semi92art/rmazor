using UnityEngine;
using UnityEditor;

namespace Games.PointsTapper.Editor
{
    [CustomEditor(typeof(PointItem))]
    public class PointItemEditor : UnityEditor.Editor
    {
        private PointItem m_Item;

        private void OnEnable()
        {
            m_Item = (PointItem) target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUI.enabled = Application.isPlaying;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Activate"))
                m_Item.Activated = true;
            if (GUILayout.Button("Deactivate"))
                m_Item.Activated = false;
            GUILayout.EndHorizontal();
        }
    }
}