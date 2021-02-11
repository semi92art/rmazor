using System;
using UnityEditor;

namespace Games.Main_Mode.Editor
{
    [CustomEditor(typeof(LevelDesigner))]
    public class LevelDesignerEditor : UnityEditor.Editor
    {
        private LevelDesigner m_Target;

        private void OnEnable()
        {
            m_Target = (LevelDesigner) target;
            
            
        }
        
        
    }
}