using System.Collections.Generic;
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
using RMAZOR.Controllers;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using MathUtils = mazing.common.Runtime.Utils.MathUtils;
using static RMAZOR.Models.ComInComArg;
using static Common.ColorIds;

namespace Editor
{
    public class ColorsEditorHelperWindow : EditorWindow
    {
        private static ColorsEditorHelperWindow _instance;

        private int                                 m_LevelIndex;
        private IGameController                     m_GameController;
        private IColorProvider                      m_ColorProvider;
        private MainColorsSetScriptableObject       m_MainColorsPropsSetScrObj;
        private AdditionalColorsSetScriptableObject m_AdditionalColorsPropsSetScrObj;
        private AdditionalColorsPropsAssetItemsSet  m_AdditionalColorsPropsSet;
        private MainColorsPropsSet                  m_MainColorsPropsSet;
        private Color?                              m_UiColor;
        private Color                               m_UiColorCheck;
        private bool                                m_ChangeOnlyHueUi = true;
        private int                                 m_CurrSetIdx;
        private Vector2                             m_ScrollPosition;

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
            EditorUtilsEx.ScrollViewZone(ref m_ScrollPosition, () =>
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
                EditorUtilsEx.GuiButtonAction(UpdateCurrentBackgroundTexture);
                EditorUtilsEx.GuiButtonAction(SetNextBackgroundTexture);
            });
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
            var coloIds = new [] {UI, UiBackground, UiBorder, UiText, UiDialogItemNormal,};
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
            var colorIdsSetItemCurrent = new[]
                {Main, PathItem, PathFill, PathBackground, UiBackground, GameUiAlternative, Background1, Background2};
            foreach (int colorId in colorIdsSetItemCurrent)
                m_ColorProvider.SetColor(colorId, GetAdditionalColor(colorId, props));
        }
        
        private static Color GetAdditionalColor(int _ColorId, AdditionalColorsPropsAssetItem _Props)
        {
            return _ColorId switch
            {
                Main              => _Props.main,
                Background1       => _Props.bacground1,
                Background2       => _Props.bacground2,
                PathItem          => _Props.GetColor(_Props.pathItemFillType),
                PathBackground    => _Props.GetColor(_Props.pathBackgroundFillType),
                PathFill          => _Props.GetColor(_Props.pathFillFillType),
                Character2        => _Props.GetColor(_Props.characterBorderFillType),
                UiBackground      => _Props.GetColor(_Props.uiBackgroundFillType),
                GameUiAlternative => _Props.GetColor(_Props.uiBackgroundFillType),
                _                 => Color.magenta
            };
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
                        EditorGUILayout.TextField(GetColorIdByName(item.name).ToString(), GUILayout.Width(80));
                        EditorGUILayout.TextField(item.name, GUILayout.Width(170));
                    });
                    var newColor = EditorGUILayout.ColorField(item.color);
                    if (newColor != item.color)
                        m_ColorProvider?.SetColor(GetColorIdByName(item.name), item.color);
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
                    _Item => GetColorIdByName(_Item.name) == UI);
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

        private void UpdateCurrentBackgroundTexture()
        {
            if (!ValidAction())
                return;
            var args = GetLevelStageArgsForBackgroundTexture(true);
            m_GameController.View.Background.OnLevelStageChanged(args);
            m_GameController.View.AdditionalBackground.OnLevelStageChanged(args);
        }

        private void SetNextBackgroundTexture()
        {
            if (!ValidAction())
                return;
            var args = GetLevelStageArgsForBackgroundTexture(false);
            m_GameController.View.Background.OnLevelStageChanged(args);
            m_GameController.View.AdditionalBackground.OnLevelStageChanged(args);
        }

        private LevelStageArgs GetLevelStageArgsForBackgroundTexture(bool _Current)
        {
            var settings = new PrefabSetManager(new AssetBundleManagerFake()).GetObject<ViewSettings>(
                CommonPrefabSetNames.Configs, "view_settings");
            int group = RmazorUtils.GetLevelsGroupIndex(m_LevelIndex);
            int levels = (_Current ? 0 : 1) * RmazorUtils.GetLevelsInGroup(group);
            m_LevelIndex = MathUtils.ClampInverse(
                m_LevelIndex + levels, 
                0, 
                400 - 1);
            var args = new Dictionary<string, object>
            {
                {KeySetBackgroundFromEditor, true}
            };
            var fakeArgs = new LevelStageArgs(
                m_LevelIndex, 
                ELevelStage.Loaded, 
                ELevelStage.Unloaded, 
                ELevelStage.ReadyToUnloadLevel,
                ELevelStage.Finished,
                float.PositiveInfinity,
                args);
            return fakeArgs;
        }

        private bool ValidAction()
        {
            if (!Application.isPlaying)
            {
                Dbg.LogWarning("This option is available only in play mode");
                return false;
            }
            m_GameController = FindObjectOfType<GameControllerMVC>();
            if (m_GameController != null)
                return true;
            Dbg.LogError("Game Controller was not found.");
            return false;
        }
    }
}
