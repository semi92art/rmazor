using Constants;
using Entities;
using GameHelpers;
using Games.RazorMaze.Views.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.Editor;

public class ColorsHelper : EditorWindow
{
    private IColorProvider                        m_ColorProvider;
    private ColorSetScriptableObject              m_ColorSetScrObj;
    private ColorSetScriptableObject.ColorItemSet m_ColorSet;
    
    [MenuItem("Tools/Colors Helper", false, 2)]
    public static void ShowWindow()
    {
        var window = GetWindow<ColorsHelper>("Color Palette Helper");
        window.minSize = new Vector2(500, 200);
    }
    
    private void OnGUI()
    {
        if (m_ColorSet == null)
        {
            m_ColorSetScrObj = PrefabUtilsEx.GetObject<ColorSetScriptableObject>(
                "views", "color_set");
            m_ColorSet = m_ColorSetScrObj.set;
        }

        EditorUtilsEx.GUIEnabledZone(false, () =>
        {
            EditorGUILayout.ObjectField(
                "set", m_ColorSetScrObj, typeof(ColorSetScriptableObject), false);
        });
        
        DisplayColors();

        if ((m_ColorProvider == null || m_ColorSetScrObj == null)
            && Application.isPlaying
            && SceneManager.GetActiveScene().name.Contains(SceneNames.Level))
        {
            m_ColorProvider = FindObjectOfType<DefaultColorProvider>();
        }
        
        EditorUtilsEx.GuiButtonAction("Save",
            () =>
            {
                m_ColorSetScrObj.set = m_ColorSet;
                AssetDatabase.SaveAssets();
            });
    }

    private void DisplayColors()
    {
        foreach (var item in m_ColorSet)
        {
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GUIEnabledZone(false, () =>
                {
                    EditorGUILayout.TextField(ColorIds.GetHash(item.name).ToString(), GUILayout.Width(80));
                    EditorGUILayout.TextField(item.name, GUILayout.Width(170));
                });
                var newColor = EditorGUILayout.ColorField(item.color);
                if (newColor != item.color)
                    m_ColorProvider?.SetColor(ColorIds.GetHash(item.name), item.color);
                item.color = newColor;
            });
        }
    }
}
