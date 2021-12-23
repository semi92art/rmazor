using System.Diagnostics;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Utils;
using Utils.Editor;

public class SavesHelper : EditorWindow
{
    [MenuItem("Tools/Saves Helper", false)]
    public static void ShowWindow()
    {
        SaveUtils.ReloadSaves();
        GetWindow<SavesHelper>("Saves Helper");
    }

    private void OnGUI()
    {
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
        EditorUtilsEx.HorizontalLine();
        EditorUtilsEx.GuiButtonAction("Open Saves File", () =>
        {
            var p = new Process {StartInfo = new ProcessStartInfo {FileName = SaveUtils.SavesPath}};
            Task.Run(() => p.Start());
        });
    }
}
