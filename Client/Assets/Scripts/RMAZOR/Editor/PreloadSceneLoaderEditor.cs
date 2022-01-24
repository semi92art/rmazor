using Common.Constants;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityToolbarExtender;

namespace RMAZOR.Editor
{
    [InitializeOnLoad]
    public static class PreloadSceneLoaderEditor
    {
        static PreloadSceneLoaderEditor()
        {
            ToolbarExtender.RightToolbarGUI.Insert(0, OnToolbarGUI);
        }

        private static void OnToolbarGUI()
        {
            var tex = EditorGUIUtility.IconContent(@"ParticleShapeTool").image;
            if (GUILayout.Button(
                new GUIContent(null, tex, "Load _preload or Prot scene"),
                "Command"))
            {
                SwitchScenes();
            }
        }
        
        [MenuItem("Tools/Switch Scenes _%s")]
        public static void SwitchScenes()
        {
            if (Application.isPlaying)
                return;
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == SceneNames.Prototyping)
                OpenPreloadScene();
            else if (sceneName == SceneNames.Preload)
                OpenProtScene();
            else
                OpenPreloadScene();
        }

        private static void OpenPreloadScene()
        {
            EditorSceneManager.OpenScene(SceneNames.FullName(SceneNames.Preload));
            EditorApplication.ExecuteMenuItem("Window/General/Game");


        }

        private static void OpenProtScene()
        {
            EditorSceneManager.OpenScene(SceneNames.FullName(SceneNames.Prototyping));
            LevelDesignerEditor.Instance.Focus();
        }
    }
}