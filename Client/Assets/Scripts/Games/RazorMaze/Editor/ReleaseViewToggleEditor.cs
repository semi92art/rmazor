using Constants;
using Games.RazorMaze.Models;
using Network;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityToolbarExtender;
using Utils;

namespace Games.RazorMaze.Editor
{
    [InitializeOnLoad]
    public static class ReleaseViewToggleEditor
    {
        private static bool _releaseView;
        private static bool _releaseViewCheck;
        
        static ReleaseViewToggleEditor()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
        }

        private static void OnToolbarGUI()
        {
            // lightMeter/greenLight
            if (SceneManager.GetActiveScene().name != SceneNames.Prototyping)
                return;
            _releaseView = GameClientUtils.GameMode == (int) EGameMode.Release;
            string iconName = _releaseView ? "lightMeter/greenLight" : "lightMeter/redLight";
            var toggleIm = EditorGUIUtility.IconContent(iconName).image;
            _releaseView = GUILayout.Toggle(
                _releaseView,  new GUIContent(
                    null, toggleIm, "Release view activation (green = release, red = prototyping)"),  "Command");
            if (_releaseView != _releaseViewCheck)
                GameClientUtils.GameMode = _releaseView ? (int) EGameMode.Release : (int) EGameMode.Prototyping;
            _releaseViewCheck = _releaseView;
        }
    }
}