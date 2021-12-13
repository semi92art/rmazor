using System.Linq;
using Constants;
using DI.Extensions;
using Entities;
using GameHelpers;
using Games.RazorMaze.Views.Common;
using Managers;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.Editor;

public class ColorsHelper : EditorWindow
{
    private IColorProvider                        m_ColorProvider;
    private ColorSetScriptableObject              m_ColorSetScrObj;
    private ColorSetScriptableObject.ColorItemSet m_ColorSet;
    private Color?                                m_UiColor;
    private Color                                 m_UiColorCheck;
    private bool                                  m_ChangeOnlyHueUi = true;
    
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
            m_ColorSetScrObj = new PrefabSetManager(new AssetBundleManagerFake()).GetObject<ColorSetScriptableObject>(
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
        
        EditorUtilsEx.HorizontalLine(Color.gray);
        GUILayout.Label("UI Color:");
        if (!m_UiColor.HasValue)
        {
            var uiSetItem = m_ColorSet.FirstOrDefault(_Item => ColorIds.GetHash(_Item.name) == ColorIds.UI);
            if (uiSetItem != null)
            {
                m_UiColor = EditorGUILayout.ColorField(uiSetItem.color);
                m_UiColorCheck = uiSetItem.color;
            }
        }
        else
        {
            m_UiColor = EditorGUILayout.ColorField(m_UiColor.Value);
        }
        m_ChangeOnlyHueUi = EditorGUILayout.Toggle("Only Hue", m_ChangeOnlyHueUi);
        if (m_UiColor.HasValue && m_UiColor != m_UiColorCheck)
            SetUiColors(m_UiColor.Value);
        if (m_UiColor.HasValue)
            m_UiColorCheck = m_UiColor.Value;

        EditorUtilsEx.HorizontalLine(Color.gray);
        
        EditorUtilsEx.GuiButtonAction("Save",
            () =>
            {
                m_ColorSetScrObj.set = m_ColorSet;
                AssetDatabase.SaveAssets();
            });
    }

    private void SetUiColors(Color _Color)
    {
        var coloIds = new []
        {
            ColorIds.UI,
            ColorIds.UiBackground,
            ColorIds.UiBorder,
            ColorIds.UiText,
            ColorIds.UiDialogItemNormal,
            ColorIds.UiDialogBackground
        };
        foreach (var id in coloIds)
        {
            var item = m_ColorSet.FirstOrDefault(_Item => ColorIds.GetHash(_Item.name) == id);
            if (item == null) 
                continue;
            var col = item.color;
            Color newColor;
            if (m_ChangeOnlyHueUi)
            {
                Color.RGBToHSV(_Color, out float h, out _, out _);
                Color.RGBToHSV(col, out _, out float s, out float v);
                newColor = Color.HSVToRGB(h, s, v).SetA(col.a);
            }
            else
            {
                newColor = _Color.SetA(col.a);
            }
            m_ColorProvider?.SetColor(id, newColor);
            item.color = newColor;
        }
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
