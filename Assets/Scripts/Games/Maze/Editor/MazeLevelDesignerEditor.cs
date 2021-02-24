using UnityEditor;

namespace Games.Maze.Editor
{
    [CustomEditor(typeof(MazeLevelDesigner))]
    public class MazeLevelDesignerEditor : UnityEditor.Editor
    {
        private MazeLevelDesigner m_Designer;
        
        private void OnEnable()
        {
            m_Designer = (MazeLevelDesigner) target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            
        }
    }
}