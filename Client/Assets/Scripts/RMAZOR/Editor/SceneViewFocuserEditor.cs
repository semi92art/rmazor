using Common.Constants;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityToolbarExtender;

namespace RMAZOR.Editor
{
    [InitializeOnLoad]
    public static class SceneViewFocuserEditor
    {
        static SceneViewFocuserEditor()
        {
            ToolbarExtender.RightToolbarGUI.Insert(1, OnToolbarGUI);
        }

        private static void OnToolbarGUI()
        {
            if (SceneManager.GetActiveScene().name != SceneNames.Prototyping)
                return;
            var tex = EditorGUIUtility.IconContent(@"UnityEditor.SceneView").image;
            if (GUILayout.Button(
                new GUIContent(null, tex, "Focus SceneView when entering play mode"),
                "Command"))
            {
                SceneView.lastActiveSceneView.in2DMode = true; 
                LevelDesignerEditor.Instance.FocusCamera(LevelDesigner.Instance.size);
            }
        }
    }
}