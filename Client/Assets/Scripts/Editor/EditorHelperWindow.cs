﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Network;
using Common.Network.Packets;
using Common.Ticker;
using Common.Utils;
using Newtonsoft.Json;
using RMAZOR;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Editor
{
    public class EditorHelperWindow : EditorWindow
    {
        private int     m_DailyBonusIndex;
        private int     m_TestUsersCount = 3;
        private string  m_DebugServerUrl;
        private string  m_TestUrlCheck;
        private int     m_GameId = -1;
        private int     m_GameIdCheck;
        private int     m_Quality = -1;
        private int     m_QualityCheck;
        private int     m_TabPage;
        private int     m_TabPageSettings;
        private Vector2 m_CommonScrollPos;
        private Vector2 m_CachedDataScrollPos;
        private Vector2 m_SettingsScrollPos;

        [MenuItem("Tools/\u2699 Editor Helper _%h", false, 0)]
        public static void ShowWindow()
        {
            GetWindow<EditorHelperWindow>("Editor Helper");
        }
    
        [MenuItem("Tools/Profiler",false, 3)]
        public static void ShowProfilerWindow()
        {
            var tProfiler = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ProfilerWindow");
            GetWindow(tProfiler, false);
        }

        private void OnEnable()
        {
            UpdateTestUrl();
            UpdateGameId();
        }

        private void OnGUI()
        {
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
                if (Application.isPlaying)
                    GUILayout.Label($"Target FPS: {Application.targetFrameRate}");
                EditorUtilsEx.HorizontalZone(() =>
                {
                    EditorUtilsEx.GuiButtonAction(CreateTestUsers, m_TestUsersCount);
                    GUILayout.Label("count:", GUILayout.Width(40));
                    m_TestUsersCount = EditorGUILayout.IntField(m_TestUsersCount);
                });
                EditorUtilsEx.GuiButtonAction("Delete test users", DeleteTestUsers);
                EditorUtilsEx.HorizontalZone(() =>
                {
                    GUILayout.Label("Debug Server Url:");
                    m_DebugServerUrl = EditorGUILayout.TextField(m_DebugServerUrl);
                    if (!string.IsNullOrEmpty(m_DebugServerUrl) && m_DebugServerUrl.Last().InRange('/','\\'))
                        m_DebugServerUrl = m_DebugServerUrl.Remove(m_DebugServerUrl.Length - 1);
                });
                EditorUtilsEx.GuiButtonAction("Set default api url", SetDefaultApiUrl);
                EditorUtilsEx.HorizontalZone(() =>
                {
                    GUILayout.Label("Quality:");
                    m_Quality = EditorGUILayout.Popup(
                        m_Quality, new[] { "Normal", "Good" });
                });
                EditorUtilsEx.HorizontalLine(Color.gray);
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
            });
            UpdateTestUrl();
            UpdateGameId();
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
            const string setName = "configs";
            static IPrefabSetManager GetPrefLoader() => new PrefabSetManager(new AssetBundleManagerFake());
            GUILayout.Label("Copy to clipboard:");
            EditorUtilsEx.GuiButtonAction("Main Colors Set", () =>
            {
                var mainColorsSetScrObj = GetPrefLoader().GetObject<MainColorsSetScriptableObject>(
                    "views", "color_set_light");
                var converter = new ColorJsonConverter();
                string json = JsonConvert.SerializeObject(
                    mainColorsSetScrObj.set,
                    Formatting.None,
                    converter);
                CommonUtils.CopyToClipboard(json);
            });
            EditorUtilsEx.GuiButtonAction("Back and Front Colors Set", () =>
            {
                var backAndFrontColorsSetScrObj = GetPrefLoader().GetObject<BackAndFrontColorsSetScriptableObject>(
                    "configs", "back_and_front_colors_set_light");
                var converter = new ColorJsonConverter();
                string json = JsonConvert.SerializeObject(
                    backAndFrontColorsSetScrObj.set,
                    Formatting.None,
                    converter);
                CommonUtils.CopyToClipboard(json);
            });
            EditorUtilsEx.GuiButtonAction("Background Lines Texture Parameters Set", () =>
            {
                var set = GetPrefLoader().GetObject<LinesTexturePropertiesSetScriptableObject>
                    (setName, "lines_texture_set");
                string json = JsonConvert.SerializeObject(set.set);
                CommonUtils.CopyToClipboard(json);
            });
            EditorUtilsEx.GuiButtonAction("Background Circles Texture Parameters Set", () =>
            {
                var set = GetPrefLoader().GetObject<CirclesTexturePropertiesSetScriptableObject>
                    (setName, "circles_texture_set");
                string json = JsonConvert.SerializeObject(set.set);
                CommonUtils.CopyToClipboard(json);
            });
            EditorUtilsEx.GuiButtonAction("Background Circles2 Texture Parameters Set", () =>
            {
                var set = GetPrefLoader().GetObject<Circles2TexturePropertiesSetScriptableObject>
                    (setName, "circles2_texture_set");
                string json = JsonConvert.SerializeObject(set.set);
                CommonUtils.CopyToClipboard(json);
            });
            EditorUtilsEx.GuiButtonAction("Background Triangles Texture Parameters Set", () =>
            {
                var set = GetPrefLoader().GetObject<TrianglesTexturePropertiesSetScriptableObject>
                    (setName, "triangles_texture_set");
                string json = JsonConvert.SerializeObject(set.set);
                CommonUtils.CopyToClipboard(json);
            });
        }

        private void ModelSettingsTabPage()
        {
            var settings = new PrefabSetManager(new AssetBundleManagerFake()).GetObject<ModelSettings>(
                "configs", "model_settings");
            SettingsTabPageCore(settings, typeof(ModelSettings));
        }
    
        private void ViewSettingsTabPage()
        {
            var settings = new PrefabSetManager(new AssetBundleManagerFake()).GetObject<ViewSettings>(
                "configs", "view_settings");
            SettingsTabPageCore(settings, typeof(ViewSettings));
        }

        private void ViewCommonGameSettingsTabPage()
        {
            var settings = new PrefabSetManager(new AssetBundleManagerFake()).GetObject<CommonGameSettings>(
                "configs", "common_game_settings");
            SettingsTabPageCore(settings, typeof(CommonGameSettings));
        }

        private void SettingsTabPageCore(Object _Settings, Type _Type)
        {
            var fieldInfos = _Type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var serObj = new SerializedObject(_Settings);
            EditorUtilsEx.ScrollViewZone(ref m_SettingsScrollPos, () =>
            {
                foreach (var fieldInfo in fieldInfos)
                {
                    var prop = serObj.FindProperty(fieldInfo.Name);
                    EditorGUILayout.PropertyField(prop);
                }
                if (!serObj.hasModifiedProperties)
                    return;
                serObj.ApplyModifiedProperties();
                EditorUtility.SetDirty(_Settings);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }, false, true);
        }

        private void UpdateTestUrl(bool _Forced = false)
        {
            if (string.IsNullOrEmpty(m_DebugServerUrl))
                m_DebugServerUrl = SaveUtilsInEditor.GetValue(SaveKeysCommon.ServerUrl);
            if (m_DebugServerUrl != m_TestUrlCheck || _Forced)
                SaveUtilsInEditor.PutValue(SaveKeysCommon.ServerUrl, m_DebugServerUrl);
            m_TestUrlCheck = m_DebugServerUrl;
        }

        private void UpdateGameId()
        {
            if (m_GameId == -1)
                m_GameId = SaveUtils.GetValue(SaveKeysCommon.GameId);
            if (m_GameId != m_GameIdCheck)
                SaveUtils.PutValue(SaveKeysCommon.GameId, m_GameId);
            m_GameIdCheck = m_GameId;
        }

        private static void CreateTestUsers(int _Count)
        {
            CommonData.Testing = true;
            var gc = new GameClient(new CommonTicker());
            const int gameId = 1;
            gc.Initialize += () =>
            {
                for (int i = 0; i < _Count; i++)
                {
                    var packet = new RegisterUserPacket(
                        new RegisterUserPacketRequestArgs
                        {
                            Name = $"{CommonUtils.GetUniqueId()}",
                            PasswordHash = "test",
                            GameId = gameId
                        });
                    int ii = i;
                    packet.OnSuccess(() =>
                        {
                            // int accId = packet.Response.Id;
                            // new GameDataField(gc, 100, accId,
                            //                   GameClientUtils.GameId, DataFieldIds.Money).Save();
                            // Dbg.Log("All test users were created successfully");
                        })
                        .OnFail(() =>
                        {
                            Dbg.LogError($"Creating test user #{ii + 1} of {_Count} failed");
                            Dbg.LogError(packet.Response);
                        });

                    gc.Send(packet);
                }
            };
            gc.Init();
        }

        private static void DeleteTestUsers()
        {
            CommonData.Testing = true;
            var gc = new GameClient(new CommonTicker());
            gc.Initialize += () =>
            {
                IPacket packet = new DeleteTestUsersPacket();
                packet.OnSuccess(() =>
                    {
                        Dbg.Log("All test users deleted");
                    })
                    .OnFail(() => Dbg.Log($"Failed to delete test users: {packet.ErrorMessage}"));
                gc.Send(packet);
            };
            gc.Init();
        }

        public static void LoadScene(string _Name)
        {
            if (Application.isPlaying)
                SceneManager.LoadScene(_Name);
            else if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(_Name);
        }

        private void SetDefaultApiUrl()
        {
            m_DebugServerUrl = @"http://77.37.152.15:7000";
            UpdateTestUrl(true);
        }

        private static Dictionary<string, string> GetAllSaveKeyValues() =>
            new Dictionary<string, string>
            {
                {"Last connection succeeded", SaveUtils.GetValue(SaveKeysCommon.LastDbConnectionSuccess).ToString()},
                {"Login", SaveUtils.GetValue(SaveKeysCommon.Login) ?? "not exist"},
                {"Password hash", SaveUtils.GetValue(SaveKeysCommon.PasswordHash) ?? "not exist"},
                {"Account id", GameClientUtils.AccountId.ToString()},
                {"Game id", SaveUtils.GetValue(SaveKeysCommon.GameId).ToString()},
            };
    }
}