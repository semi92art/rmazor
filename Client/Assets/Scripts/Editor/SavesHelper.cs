using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Common;
using Common.Utils;
using RMAZOR;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class SavesHelper : EditorWindow
    {
        [MenuItem("Tools/Saves Helper", false, 107)]
        public static void ShowWindow()
        {
            SaveUtils.CreateSavesFileIfNotExist();
            SaveUtils.ReloadSaves();
            GetWindow<SavesHelper>("Saves Helper");
        }

        private void OnFocus()
        {
            SaveUtils.CreateSavesFileIfNotExist();
            SaveUtils.ReloadSaves();
        }

        private void OnGUI()
        {
            EditorUtilsEx.GuiButtonAction("Open Saves File Location", () =>
            {
                var p = new Process {StartInfo = new ProcessStartInfo {FileName = Application.persistentDataPath}};
                Task.Run(() => p.Start());
            });
            EditorUtilsEx.GuiButtonAction("Reset static variables", () =>
            {
                SaveKeysRmazor.ResetState();
                SaveKeysCommon.ResetState();
            });
            EditorUtilsEx.GuiButtonAction("Reload Saves File", SaveUtils.ReloadSaves);
            EditorUtilsEx.GuiButtonAction("Delete Saves File", () =>
            {
                if(File.Exists(SaveUtils.SavesPath))
                    File.Delete(SaveUtils.SavesPath);
            });
            EditorUtilsEx.HorizontalLine();
            GUILayout.Label("Save file content:");
            var elements = SaveUtils.SavesDoc.Root?.Elements();
            if (elements == null)
                return;
            float w = position.width;
            float nameWidth = w * 0.3f;
            float valueWidth = w - nameWidth;
            EditorUtilsEx.GUIEnabledZone(false, () =>
            {
                foreach (var element in elements)
                {
                    EditorUtilsEx.HorizontalZone(() =>
                    {
                        GUILayout.TextField(element.Name.LocalName, GUILayout.Width(nameWidth));
                        GUILayout.TextField(element.Value, GUILayout.Width(valueWidth));
                    });
                }
            });
        }
    }
}
