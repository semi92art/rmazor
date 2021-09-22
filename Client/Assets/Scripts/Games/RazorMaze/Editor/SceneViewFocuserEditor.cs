using Constants;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityToolbarExtender;

namespace Games.RazorMaze.Editor
{
    [InitializeOnLoad]
    public static class SceneViewFocuserEditor
    {
        static SceneViewFocuserEditor()
        {
            ToolbarExtender.RightToolbarGUI.Insert(0, OnToolbarGUI);
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
                LevelDesignerEditor.FocusCamera(LevelDesigner.Instance.size);
            }
        }
    }
}