using RMAZOR;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TestBackgroundCreator))]
public class TestBackgroundCreatorEditor : UnityEditor.Editor
{
    private TestBackgroundCreator m_O;
    
    private void OnEnable()
    {
        m_O = target as TestBackgroundCreator;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Set Bounds"))
        {
            m_O.SetBounds();
        }
        
        if (GUILayout.Button("Get UV"))
        {
            m_O.GetUV();
        }
        
        if (GUILayout.Button("Set Stencil Ref"))
        {
            m_O.SetStencilRef();
        }
    }
}
