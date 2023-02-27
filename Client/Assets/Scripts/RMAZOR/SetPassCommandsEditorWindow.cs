#if UNITY_EDITOR
using mazing.common.Runtime.Utils;
using Newtonsoft.Json;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Controllers;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class SetPassCommandsEditorWindow : EditorWindow
    {
        private static MazeInfo _levelInfo;
        private static string   _passCommandsSerialized;
        private static Vector2  _scrollPos; 
        
        public static void ShowWindow(MazeInfo _LevelInfo)
        {
            _levelInfo = _LevelInfo;
            _passCommandsSerialized = _levelInfo.AdditionalInfo.PassCommandsRecord == null
                ? string.Empty
                : JsonConvert.SerializeObject(_levelInfo.AdditionalInfo.PassCommandsRecord);
            var window = GetWindow<SetPassCommandsEditorWindow>("Level Pass Commands");
            window.minSize = window.maxSize = new Vector2(300, EditorGUIUtility.singleLineHeight * 3.5f);
        }

        private void OnGUI()
        {
            EditorUtilsEx.ScrollViewZone(ref _scrollPos, () =>
            {
                var contentHeight = GUILayout.Height(position.height - EditorGUIUtility.singleLineHeight);
                _passCommandsSerialized = EditorGUILayout.TextArea(_passCommandsSerialized, contentHeight);    
            });
            
            EditorUtilsEx.GuiButtonAction("Save", () =>
            {
                var passCommands = JsonConvert.DeserializeObject<InputCommandsRecord>(_passCommandsSerialized);
                _levelInfo.AdditionalInfo.PassCommandsRecord = passCommands;
            });
        }
    }
}
#endif