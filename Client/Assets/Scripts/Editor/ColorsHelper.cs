using System.Linq;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Extensions;
using Common.Managers;
using Common.Providers;
using Common.Utils;
using Newtonsoft.Json;
using RMAZOR;
using RMAZOR.Views.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor
{
    public class ColorsHelper : EditorWindow
    {
        private IColorProvider                        m_ColorProvider;
        private MainColorsSetScriptableObject              m_MainColorsSetScrObj;
        private BackAndFrontColorsSetScriptableObject m_BackAndFrontColorsSetScrObj;
        private BackAndFrontColorsSet                 m_BackAndFrontColorsSet;
        private MainColorsItemSet                          m_MainColorsSet;
        private Color?                                m_UiColor;
        private Color                                 m_UiColorCheck;
        private bool                                  m_ChangeOnlyHueUi = true;
    
        [MenuItem("Tools/Colors Helper _%&c", false, 2)]
        public static void ShowWindow()
        {
            var window = GetWindow<ColorsHelper>("Color Palette Helper");
            window.minSize = new Vector2(500, 200);
        }
    
        private void OnGUI()
        {
            if (m_MainColorsSet == null || m_BackAndFrontColorsSet == null)
                LoadSets();
            DisplayColorSetObjectFields();
            EditorUtilsEx.GuiButtonAction(CopyMainColorsToClipboard);
            EditorUtilsEx.GuiButtonAction(CopyBackAndFrontColorsToClipboard);
            DisplayColors();
            if ((m_ColorProvider == null || m_MainColorsSetScrObj == null)
                && Application.isPlaying
                && SceneManager.GetActiveScene().name.Contains(SceneNames.Level))
            {
                m_ColorProvider = FindObjectOfType<ColorProvider>();
            }
            EditorUtilsEx.HorizontalLine(Color.gray);
            GUILayout.Label("UI Color:");
            if (!m_UiColor.HasValue)
            {
                var uiSetItem = m_MainColorsSet.FirstOrDefault(
                    _Item => ColorIdsCommon.GetHash(_Item.name) == ColorIdsCommon.UI);
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
        }

        private void LoadSets()
        {
            var manager = new PrefabSetManager(new AssetBundleManagerFake());
            m_MainColorsSetScrObj = manager.GetObject<MainColorsSetScriptableObject>(
                "views", "color_set_light");
            m_MainColorsSet = m_MainColorsSetScrObj.set;
            m_BackAndFrontColorsSetScrObj = manager.GetObject<BackAndFrontColorsSetScriptableObject>(
                "configs", "back_and_front_colors_set_light");
            m_BackAndFrontColorsSet = m_BackAndFrontColorsSetScrObj.set;
        }

        private void DisplayColorSetObjectFields()
        {
            m_MainColorsSetScrObj = EditorGUILayout.ObjectField(
                "main set", 
                m_MainColorsSetScrObj, 
                typeof(MainColorsSetScriptableObject), 
                false) as MainColorsSetScriptableObject;
            m_BackAndFrontColorsSetScrObj = EditorGUILayout.ObjectField(
                "back and front set",
                m_BackAndFrontColorsSetScrObj,
                typeof(BackAndFrontColorsSetScriptableObject),
                false) as BackAndFrontColorsSetScriptableObject;
        }

        private void SetUiColors(Color _Color)
        {
            var coloIds = new []
            {
                ColorIdsCommon.UI,
                ColorIdsCommon.UiBackground,
                ColorIdsCommon.UiBorder,
                ColorIdsCommon.UiText,
                ColorIdsCommon.UiDialogItemNormal,
                ColorIdsCommon.UiDialogBackground
            };
            foreach (int id in coloIds)
            {
                var item = m_MainColorsSet.FirstOrDefault(_Item => ColorIdsCommon.GetHash(_Item.name) == id);
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
            foreach (var item in m_MainColorsSet)
            {
                EditorUtilsEx.HorizontalZone(() =>
                {
                    EditorUtilsEx.GUIEnabledZone(false, () =>
                    {
                        EditorGUILayout.TextField(ColorIdsCommon.GetHash(item.name).ToString(), GUILayout.Width(80));
                        EditorGUILayout.TextField(item.name, GUILayout.Width(170));
                    });
                    var newColor = EditorGUILayout.ColorField(item.color);
                    if (newColor != item.color)
                        m_ColorProvider?.SetColor(ColorIdsCommon.GetHash(item.name), item.color);
                    item.color = newColor;
                });
            }
        }
        
        private void CopyMainColorsToClipboard()
        {
            var converter = new MainColorItemsSetConverter();
            string json = JsonConvert.SerializeObject(
                m_MainColorsSet,
                converter);
            CommonUtils.CopyToClipboard(json);
        }

        private void CopyBackAndFrontColorsToClipboard()
        {
            var converter = new BackAndFrontColorsSetConverter();
            string json = JsonConvert.SerializeObject(
                m_BackAndFrontColorsSet,
                converter);
            CommonUtils.CopyToClipboard(json);
        }
    }
}
