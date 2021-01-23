using UnityEditor;
using UnityEngine;

namespace UI.Editor
{
    [CustomEditor(typeof(PrototypingUiDebugPanel))]
    public class PrototypingUiPanelEditor : UnityEditor.Editor
    {
        private PrototypingUiDebugPanel m_Target;

        private void OnEnable()
        {
            m_Target = target as PrototypingUiDebugPanel;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Create Button"))
            {
                m_Target.AddButton();
            }
        }
    }
}