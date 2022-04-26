using Common;
using Common.Constants;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityToolbarExtender;

namespace RMAZOR.Editor
{
    public static class SceneViewFocuserEditor
    {
        [InitializeOnLoadMethod]
        public static void AddButtonToToolbar()
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
                Dbg.Log("Focus");
                SceneView.lastActiveSceneView.in2DMode = true; 
                LevelDesignerEditor.Instance.FocusCamera(LevelDesigner.Instance.size);
            }
        }
    }
}