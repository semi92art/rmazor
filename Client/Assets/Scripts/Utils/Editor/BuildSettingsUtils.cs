using Constants;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace Utils.Editor
{
    public static class BuildSettingsUtils
    {
        [MenuItem("Tools/Build Settings/Set default build scene list")]
        public static void AddDefaultScenesToBuild()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"Assets/Scenes/{SceneNames.Preload}.unity", true),
                new EditorBuildSettingsScene($"Assets/Scenes/{SceneNames.Level}.unity", true)
            };
        }

        [MenuItem("Tools/Build Settings/Add current scene in build")]
        public static void AddOpenedSceneToBuild()
        {
            string path = SceneManager.GetActiveScene().path;
            EditorBuildSettings.scenes = new[] {new EditorBuildSettingsScene(path, true)};
        }
    }
}