using System.Linq;
using System.Runtime.CompilerServices;
using Common;
using Common.Constants;
using Common.Entities;
using mazing.common.Runtime;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Utils;
using Newtonsoft.Json;
using RMAZOR;
using RMAZOR.Views.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using MathUtils = mazing.common.Runtime.Utils.MathUtils;

namespace Editor
{
    public class ColorsEditorHelperWindow : EditorWindow
    {
        private static ColorsEditorHelperWindow _instance;

        private IColorProvider                      m_ColorProvider;
        private MainColorsSetScriptableObject       m_MainColorsPropsSetScrObj;
        private AdditionalColorsSetScriptableObject m_AdditionalColorsPropsSetScrObj;
        private AdditionalColorsPropsAssetItemsSet  m_AdditionalColorsPropsSet;
        private MainColorsPropsSet                  m_MainColorsPropsSet;
        private Color?                              m_UiColor;
        private Color                               m_UiColorCheck;
        private bool                                m_ChangeOnlyHueUi = true;
        private int                                 m_CurrSetIdx;

        [MenuItem("Tools/Colors Helper _%&c", false, 102)]
        public static void ShowWindow()
        {
            var window = GetWindow<ColorsEditorHelperWindow>("Color Palette Helper");
            window.minSize = new Vector2(500, 200);
            _instance = window;
            _instance.LoadSets();
        }

        [InitializeOnLoadMethod]
        private static void OnInitializeOnLoad()
        {
            static void OnSceneLoaded(Scene _Scene, LoadSceneMode _Mode)
            {
                if (_Scene.name != SceneNames.Level || _instance == null) 
                    return;
                _instance.m_ColorProvider = FindObjectOfType<ColorProvider>();
                _instance.LoadSets();
            }
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnFocus()
        {
            _instance = this;
            LoadColorsProvider(true);
        }

        private void OnGUI()
        {
            LoadColorsProvider(false);
            DisplayColorSetObjectFields();
            EditorUtilsEx.GuiButtonAction(CopyMainColorsToClipboard);
            EditorUtilsEx.GuiButtonAction(CopyAdditionalColorsToClipboard);
            EditorUtilsEx.HorizontalZone(() =>
            {
                EditorUtilsEx.GuiButtonAction("Update color set",   SetCurrentAdditionalColorSet);
                EditorUtilsEx.GuiButtonAction("Previous color set", SetPreviousAdditionalColorSet);
                EditorUtilsEx.GuiButtonAction("Next color set",     SetNextAdditionalColorSet);
            });
            DisplayColors();
            EditorUtilsEx.HorizontalLine(Color.gray);
            DisplayUiColorsEditorZone();
            EditorUtilsEx.HorizontalLine(Color.gray);
        }

        private void LoadColorsProvider(bool _Forced)
        {
            if ((m_ColorProvider == null || _Forced)
                && Application.isPlaying
                && SceneManager.GetActiveScene().name.Contains(SceneNames.Level))
            {
                m_ColorProvider = FindObjectOfType<ColorProvider>();
            }
        }

        private void LoadSets()
        {
            var manager = new PrefabSetManager(new AssetBundleManagerFake());
            m_MainColorsPropsSetScrObj = manager.GetObject<MainColorsSetScriptableObject>(
                CommonPrefabSetNames.Views, "color_set_light");
            m_MainColorsPropsSet = m_MainColorsPropsSetScrObj.set;
            m_AdditionalColorsPropsSetScrObj = manager.GetObject<AdditionalColorsSetScriptableObject>(
                CommonPrefabSetNames.Configs, "additional_colors_set");
            m_AdditionalColorsPropsSet = m_AdditionalColorsPropsSetScrObj.set;
        }

        private void DisplayColorSetObjectFields()
        {
            m_MainColorsPropsSetScrObj = EditorGUILayout.ObjectField(
                "main set", 
                m_MainColorsPropsSetScrObj, 
                typeof(MainColorsSetScriptableObject), 
                false) as MainColorsSetScriptableObject;
            m_AdditionalColorsPropsSetScrObj = EditorGUILayout.ObjectField(
                "back and front set",
                m_AdditionalColorsPropsSetScrObj,
                typeof(AdditionalColorsSetScriptableObject),
                false) as AdditionalColorsSetScriptableObject;
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
            };
            foreach (int id in coloIds)
            {
                var item = m_MainColorsPropsSet.FirstOrDefault(
                    _Item => ColorIds.GetColorIdByName(_Item.name) == id);
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
        
        private void SetPreviousAdditionalColorSet()
        {
            SetNextOrPreviousOrCurrentAdditionalColorSet(false);
            SetAdditionalColorSetColors();
        }

        private void SetNextAdditionalColorSet()
        {
            SetNextOrPreviousOrCurrentAdditionalColorSet(true);
            SetAdditionalColorSetColors();
        }
        
        private void SetCurrentAdditionalColorSet()
        {
            SetNextOrPreviousOrCurrentAdditionalColorSet(null);
            SetAdditionalColorSetColors();
        }

        private void SetAdditionalColorSetColors()
        {
            if (!Application.isPlaying)
            {
                Dbg.LogWarning("This option is available only in play mode");
                return;
            }
            if (m_ColorProvider == null)
            {
                Dbg.LogError("Color provider is null");
                return;
            }
            var props = m_AdditionalColorsPropsSet[m_CurrSetIdx];
            CommonDataRmazor.CameraEffectsCustomAnimator?.SetBloom(props.bloom);
            CommonDataRmazor.BackgroundTextureControllerRmazor?.SetAdditionalInfo(props.additionalInfo);
            CommonDataRmazor.AdditionalBackgroundDrawer?.SetAdditionalBackgroundSprite(props.additionalInfo.additionalBackgroundName);
            m_ColorProvider.SetColor(ColorIds.MoneyItem,         m_ColorProvider.GetColor(ColorIds.MoneyItem));
            m_ColorProvider.SetColor(ColorIds.Main,              props.main);
            m_ColorProvider.SetColor(ColorIds.Background1,       props.bacground1);
            m_ColorProvider.SetColor(ColorIds.Background2,       props.bacground2);
            m_ColorProvider.SetColor(ColorIds.PathItem,          props.GetColor(props.pathItemFillType));
            m_ColorProvider.SetColor(ColorIds.PathBackground,    props.GetColor(props.pathBackgroundFillType));
            m_ColorProvider.SetColor(ColorIds.PathFill,          GetAdditionalColorPathItemFill(props));
            m_ColorProvider.SetColor(ColorIds.Character2,        props.GetColor(props.characterBorderFillType));
            m_ColorProvider.SetColor(ColorIds.UiBackground,      props.GetColor(props.uiBackgroundFillType));
            m_ColorProvider.SetColor(ColorIds.GameUiAlternative, props.GetColor(props.uiBackgroundFillType));
        }

        private void SetNextOrPreviousOrCurrentAdditionalColorSet(bool? _Next)
        {
            int addict = 0;
            if (_Next.HasValue)
                addict = _Next.Value ? 1 : -1;
            void AddAddict()
            {
                m_CurrSetIdx = MathUtils.ClampInverse(
                    m_CurrSetIdx + addict, 0, m_AdditionalColorsPropsSet.Count - 1);
            }
            AddAddict();
            if (_Next.HasValue)
            {
                while (!m_AdditionalColorsPropsSet[m_CurrSetIdx].inUse)
                    AddAddict();
            }
        }

        private void DisplayColors()
        {
            if (m_MainColorsPropsSet == null)
                LoadSets();
            GUILayout.Label("Main Colors Set:");
            foreach (var item in m_MainColorsPropsSet)
            {
                EditorUtilsEx.HorizontalZone(() =>
                {
                    EditorUtilsEx.GUIEnabledZone(false, () =>
                    {
                        EditorGUILayout.TextField(ColorIds.GetColorIdByName(item.name).ToString(), GUILayout.Width(80));
                        EditorGUILayout.TextField(item.name, GUILayout.Width(170));
                    });
                    var newColor = EditorGUILayout.ColorField(item.color);
                    if (newColor != item.color)
                        m_ColorProvider?.SetColor(ColorIds.GetColorIdByName(item.name), item.color);
                    item.color = newColor;
                });
            }
        }

        private void DisplayUiColorsEditorZone()
        {
            GUILayout.Label("UI Color:");
            if (!m_UiColor.HasValue)
            {
                var uiSetItem = m_MainColorsPropsSet.FirstOrDefault(
                    _Item => ColorIds.GetColorIdByName(_Item.name) == ColorIds.UI);
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
        }
        
        private void CopyMainColorsToClipboard()
        {
            var converter = new ColorJsonConverter();
            string json = JsonConvert.SerializeObject(
                m_MainColorsPropsSet,
                converter);
            CommonUtils.CopyToClipboard(json);
        }

        private void CopyAdditionalColorsToClipboard()
        {
            var converter = new ColorJsonConverter();
            string json = JsonConvert.SerializeObject(
                m_AdditionalColorsPropsSet,
                converter);
            CommonUtils.CopyToClipboard(json);
        }
        
        private static Color GetAdditionalColorPathItemFill(AdditionalColorsPropsAssetItem _Props)
        {
            if (!SRLauncher.ViewSettings.mazeItemBlockColorEqualsMainColor)
                return _Props.GetColor(_Props.pathFillFillType);
            EBackAndFrontColorType colorType = _Props.pathFillFillType switch
            {
                EBackAndFrontColorType.Main => _Props.pathBackgroundFillType switch
                {
                    EBackAndFrontColorType.Main        => _Props.pathFillFillType,
                    EBackAndFrontColorType.Background1 => EBackAndFrontColorType.Background2,
                    EBackAndFrontColorType.Background2 => EBackAndFrontColorType.Background1,
                    _                                  => throw new SwitchExpressionException(_Props.pathBackgroundFillType)
                },
                EBackAndFrontColorType.Background1 => EBackAndFrontColorType.Background1,
                EBackAndFrontColorType.Background2 => EBackAndFrontColorType.Background2,
                _                                  => throw new SwitchExpressionException(_Props.pathFillFillType)
            };
            return _Props.GetColor(colorType);
        }
    }
}
