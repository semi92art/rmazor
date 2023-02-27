using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Helpers;
using Common.Managers;
using Common.Managers.Advertising;
using Common.Managers.Notifications;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders.Camera_Effects_Props;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Managers.Notifications;
using mazing.common.Runtime.Utils;
using Newtonsoft.Json;
using RMAZOR;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Editor
{
    public class EditorHelperWindow : EditorWindow
    {
        private int                m_DailyBonusIndex;
        private string             m_DebugServerUrl;
        private string             m_TestUrlCheck;
        private int                m_TabPage;
        private int                m_TabPageSettings;
        private Vector2            m_CommonScrollPos;
        private Vector2            m_CachedDataScrollPos;
        private Vector2            m_SettingsScrollPos;
        private static GlobalGameSettings _globalGameSettings;

        private SerializedObject m_SettingsSerObj;

        [MenuItem("Tools/\u2699 Editor Helper _%h", false, 104)]
        public static void ShowWindow()
        {
            GetWindow<EditorHelperWindow>("Editor Helper");
        }
    
        [MenuItem("Tools/Profiler",false, 105)]
        public static void ShowProfilerWindow()
        {
            var tProfiler = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ProfilerWindow");
            GetWindow(tProfiler, false);
        }

        private void OnEnable()
        {
            UpdateTestUrl();
        }

        private void OnGUI()
        {
            if (_globalGameSettings == null)
            {
                _globalGameSettings =
                    AssetDatabase.LoadAssetAtPath<GlobalGameSettings>(
                        "Assets/Prefabs/Configs and Sets/common_game_settings.asset");
            }
            m_TabPage = GUILayout.Toolbar (
                m_TabPage, 
                new [] {"Common", "Cached Data", "Settings", "Remote"});
            switch (m_TabPage) 
            {
                case 0:  CommonTabPage();     break;
                case 1:  CachedDataTabPage(); break;
                case 2:  SettingsTabPage();   break;
                case 3:  RemoteTabPage();     break;
                default: throw new SwitchCaseNotImplementedException(m_TabPage);
            }
        }

        private void CommonTabPage()
        {
            EditorUtilsEx.ScrollViewZone(ref m_CommonScrollPos, () =>
            {
                var bt = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android
                    ? NamedBuildTarget.Android : NamedBuildTarget.iOS;
                EditorUtilsEx.HorizontalZone(() =>
                {
                    EditorUtilsEx.GuiButtonAction("Add Admob", () => BuildTargetChangeListener.AddGoogleAds(bt));
                    EditorUtilsEx.GuiButtonAction("Remove Admob", () => BuildTargetChangeListener.RemoveGoogleAds(bt));
                });
                EditorUtilsEx.HorizontalZone(() =>
                {
                    EditorUtilsEx.GuiButtonAction("Add Appodeal", () => BuildTargetChangeListener.AddAppodeal(bt));
                    EditorUtilsEx.GuiButtonAction("Remove Appodeal", () => BuildTargetChangeListener.RemoveAppodeal(bt));
                });
                EditorUtilsEx.HorizontalZone(() =>
                {
                    EditorUtilsEx.GuiButtonAction("Add UnityAds", () => BuildTargetChangeListener.AddUnityAds(bt));
                    EditorUtilsEx.GuiButtonAction("Remove UnityAds", () => BuildTargetChangeListener.RemoveUnityAds(bt));
                });
                EditorUtilsEx.HorizontalLine();
                EditorUtilsEx.HorizontalZone(() =>
                {
                    EditorUtilsEx.GuiButtonAction("Copy Appodeal settings from backup", BuildTargetChangeListener.CopyAppodealSettingsFromBackup);
                    EditorUtilsEx.GuiButtonAction("Remove Appodeal settings folder", BuildTargetChangeListener.RemoveAppodealSettingsFolder);
                });
                EditorUtilsEx.HorizontalLine();
                EditorUtilsEx.HorizontalZone(() =>
                {
                    EditorUtilsEx.GuiButtonAction("Set Nice Vibrations v.3.9", () => BuildTargetChangeListener.SetNiceVibrationsPluginV39(bt));
                    EditorUtilsEx.GuiButtonAction("Set Nice Vibrations v.4.1", () => BuildTargetChangeListener.SetNiceVibrationsPluginV41(bt));
                });
                EditorUtilsEx.HorizontalLine();
                var headerStyle = new GUIStyle
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    normal = {textColor = GUI.contentColor}
                };
                GUILayout.Label("Scenes", headerStyle);
                var sceneGuids = AssetDatabase.FindAssets(
                    "l:Scene t:Scene", 
                    new[] {SceneNames.GetScenesPath()});
                foreach (string scenePath in sceneGuids.Select(AssetDatabase.GUIDToAssetPath))
                {
                    EditorUtilsEx.GuiButtonAction(
                        scenePath.Replace("Assets/Scenes/",
                                          string.Empty).Replace(".unity", string.Empty),
                        LoadScene, 
                        scenePath,
                        GUILayout.Height(30f));
                }

                var lastLevelStageArgs = CommonDataRmazor.LastLevelStageArgs;
                if (CommonDataRmazor.LastLevelStageArgs == null)
                    return;
                GUILayout.Label("Level Index: " + lastLevelStageArgs.LevelIndex);
                GUILayout.Label("Current Level Stage: " + lastLevelStageArgs.LevelStage);
                GUILayout.Label("Previous Level Stage: " + lastLevelStageArgs.PreviousStage);
                GUILayout.Label("PrePrevious Level Stage: " + lastLevelStageArgs.PrePreviousStage);
                GUILayout.Label("PrePrePrevious Level Stage: " + lastLevelStageArgs.PrePrePreviousStage);
                if (lastLevelStageArgs.Arguments == null)
                {
                    GUILayout.Label("Args is null");
                }
                else
                {
                    foreach ((string key, var value) in lastLevelStageArgs.Arguments)
                        GUILayout.Label(key + ": " + value);
                }
            });
            UpdateTestUrl();
        }

        private void CachedDataTabPage()
        {
            EditorUtilsEx.ScrollViewZone(ref m_CachedDataScrollPos, () =>
            {
                GUILayout.Label("Cached data:");
                foreach (var skVal in GetAllSaveKeyValues())
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(skVal.Key, GUILayout.Width(200));
                    Color defCol = GUI.contentColor;
                    GUI.contentColor = skVal.Value switch
                    {
                        "empty"     => Color.yellow,
                        "not exist" => Color.red,
                        _           => GUI.contentColor
                    };
                    GUILayout.Label(skVal.Value);
                    GUI.contentColor = defCol;
                    GUILayout.EndHorizontal();
                }
            });
        }

        private void SettingsTabPage()
        {
            m_TabPageSettings = GUILayout.Toolbar (
                m_TabPageSettings, 
                new [] {"Model Settings", "View Settings", "Common Settings"});
            switch (m_TabPageSettings) 
            {
                case 0:  ModelSettingsTabPage();          break;
                case 1:  ViewSettingsTabPage();           break;
                case 2:  ViewCommonGameSettingsTabPage(); break;
                default: throw new SwitchCaseNotImplementedException(m_TabPageSettings);
            }
        }

        private static void RemoteTabPage()
        {
            static IPrefabSetManager GetPrefLoader() => new PrefabSetManager(new AssetBundleManagerFake());
            GUILayout.Label("Copy to clipboard:");
            EditorUtilsEx.GuiButtonAction("Main Colors Set", () =>
            {
                var mainColorsSetScrObj = GetPrefLoader().GetObject<MainColorsSetScriptableObject>(
                    CommonPrefabSetNames.Views, "color_set_light");
                var converter = new ColorJsonConverter();
                string json = JsonConvert.SerializeObject(
                    mainColorsSetScrObj.set,
                    Formatting.None,
                    converter);
                CommonUtils.CopyToClipboard(json);
            });
            EditorUtilsEx.GuiButtonAction("Additional Colors Set", () =>
            {
                var backAndFrontColorsSetScrObj = GetPrefLoader().GetObject<AdditionalColorsSetScriptableObject>(
                    CommonPrefabSetNames.Configs, "additional_colors_set");
                var converter = new ColorJsonConverter();
                string json = JsonConvert.SerializeObject(
                    backAndFrontColorsSetScrObj.set,
                    Formatting.None,
                    converter);
                CommonUtils.CopyToClipboard(json);
            });
            EditorUtilsEx.GuiButtonAction("Background Triangles 2 Texture Parameters Set", () =>
            {
                var set = GetPrefLoader().GetObject<Triangles2TexturePropsSetScriptableObject>
                    (CommonPrefabSetNames.Configs, "triangles2_texture_set");
                string json = JsonConvert.SerializeObject(set.set);
                CommonUtils.CopyToClipboard(json);
            });
            EditorUtilsEx.GuiButtonAction("Default ads providers set", () =>
            {
                var set = new List<AdProviderInfo>
                {
                    new AdProviderInfo
                    {
                        Enabled = true,
                        ShowRate = 100f,
                        Source = AdvertisingNetworks.Admob,
                        Platform = RuntimePlatform.Android
                    },
                    new AdProviderInfo
                    {
                        Enabled = true,
                        ShowRate = 100f,
                        Source = AdvertisingNetworks.UnityAds,
                        Platform = RuntimePlatform.Android
                    },
                    new AdProviderInfo
                    {
                        Enabled = true,
                        ShowRate = 100f,
                        Source = AdvertisingNetworks.Appodeal,
                        Platform = RuntimePlatform.Android
                    },
                    new AdProviderInfo
                    {
                        Enabled = true,
                        ShowRate = 100f,
                        Source = AdvertisingNetworks.Admob,
                        Platform = RuntimePlatform.IPhonePlayer
                    },
                    new AdProviderInfo
                    {
                        Enabled = true,
                        ShowRate = 100f,
                        Source = AdvertisingNetworks.UnityAds,
                        Platform = RuntimePlatform.IPhonePlayer
                    },
                    new AdProviderInfo
                    {
                        Enabled = true,
                        ShowRate = 100f,
                        Source = AdvertisingNetworks.Appodeal,
                        Platform = RuntimePlatform.IPhonePlayer
                    }
                };
                string json = JsonConvert.SerializeObject(set);
                CommonUtils.CopyToClipboard(json);
            });

            EditorUtilsEx.GuiButtonAction("Default notifications set", () =>
            {
                var set = DefaultNotificationsGetter.GetNotifications();
                string json = JsonConvert.SerializeObject(set);
                CommonUtils.CopyToClipboard(json);
            });
            
            EditorUtilsEx.GuiButtonAction("Default color grading props set", () =>
            {
                var colorGradingProps = new ColorGradingProps
                {
                    Contrast       = 0.35f,
                    Blur           = 0.2f,
                    Saturation     = 0.3f,
                    VignetteAmount = 0.05f
                };
                string json = JsonConvert.SerializeObject(colorGradingProps);
                CommonUtils.CopyToClipboard(json);
            });
            EditorUtilsEx.GuiButtonAction("Default test device ids for admob set", () =>
            {
                var ids = new List<string>
                {
                    "FE989E63CDFFFB64D9E6A288C137E1E2",
                    "16DB30D44D674B09110966E0648E783D",
                    "A7BDF038439EAB9E32977E9485093F43",
                    "B648AEC33D2557D0BFD1FD486B3E7678",
                    "7D516587810FD383626B5C782FB4078A",
                    "CCF954D464D11BD437D3885E0CDAE854"
                };
                string json = JsonConvert.SerializeObject(ids);
                CommonUtils.CopyToClipboard(json);
            });
        }

        private void ModelSettingsTabPage()
        {
            var settings = new PrefabSetManager(new AssetBundleManagerFake()).GetObject<ModelSettings>(
                CommonPrefabSetNames.Configs, "model_settings");
            SettingsTabPageCore(settings, typeof(ModelSettings));
        }
    
        private void ViewSettingsTabPage()
        {
            var settings = new PrefabSetManager(new AssetBundleManagerFake()).GetObject<ViewSettings>(
                CommonPrefabSetNames.Configs, "view_settings");
            SettingsTabPageCore(settings, typeof(ViewSettings));
        }

        private void ViewCommonGameSettingsTabPage()
        {
            var settings = new PrefabSetManager(new AssetBundleManagerFake()).GetObject<GlobalGameSettings>(
                CommonPrefabSetNames.Configs, "common_game_settings");
            SettingsTabPageCore(settings, typeof(GlobalGameSettings));
        }

        private void SettingsTabPageCore(Object _Settings, Type _Type)
        {
            if (m_SettingsSerObj == null || m_SettingsSerObj.GetType() != _Type)
                m_SettingsSerObj = new SerializedObject(_Settings);
            else 
                m_SettingsSerObj.Update();
            var fieldInfos = _Type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            EditorUtilsEx.ScrollViewZone(ref m_SettingsScrollPos, () =>
            {
                foreach (var fieldInfo in fieldInfos)
                {
                    var prop = m_SettingsSerObj.FindProperty(fieldInfo.Name);
                    EditorGUILayout.PropertyField(prop);
                }
                if (!m_SettingsSerObj.hasModifiedProperties)
                    return;
                m_SettingsSerObj.ApplyModifiedProperties();
                EditorUtility.SetDirty(_Settings);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }, false, true);
        }

        private void UpdateTestUrl(bool _Forced = false)
        {
            if (string.IsNullOrEmpty(m_DebugServerUrl))
                m_DebugServerUrl = SaveUtilsInEditor.GetValue(SaveKeysMazor.ServerUrl);
            if (m_DebugServerUrl != m_TestUrlCheck || _Forced)
                SaveUtilsInEditor.PutValue(SaveKeysMazor.ServerUrl, m_DebugServerUrl);
            m_TestUrlCheck = m_DebugServerUrl;
        }
        
        public static void LoadScene(string _Name)
        {
            if (Application.isPlaying)
                SceneManager.LoadScene(_Name);
            else if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(_Name);
        }
        
        private static Dictionary<string, string> GetAllSaveKeyValues()
        {
            return new Dictionary<string, string>
            {
                {"Last connection succeeded", SaveUtils.GetValue(SaveKeysCommon.LastDbConnectionSuccess).ToString()},
                {"Login", SaveUtils.GetValue(SaveKeysMazor.Login) ?? "not exist"},
                {"Password hash", SaveUtils.GetValue(SaveKeysMazor.PasswordHash) ?? "not exist"},
                {"Account id", GameClientUtils.AccountId.ToString()},
            };
        }
    }
}