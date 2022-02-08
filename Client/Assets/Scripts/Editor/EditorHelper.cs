using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common;
using Common.Constants;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Network;
using Common.Network.Packets;
using Common.Ticker;
using Common.Utils;
using GameHelpers;
using RMAZOR;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.Editor;
using Object = UnityEngine.Object;

namespace Editor
{
    public class EditorHelper : EditorWindow
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
        private Vector2 m_CommonScrollPos;
        private Vector2 m_CachedDataScrollPos;
        private Vector2 m_ViewSettingsScrollPos;

        [MenuItem("Tools/\u2699 Editor Helper _%h", false, 0)]
        public static void ShowWindow()
        {
            GetWindow<EditorHelper>("Editor Helper");
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
            UpdateQuality();
        }

        private void OnGUI()
        {
            m_TabPage = GUILayout.Toolbar (
                m_TabPage, 
                new [] {"Common", "Cached Data", "Model Settings", "View Settings", "Common Settings"});
            switch (m_TabPage) 
            {
                case 0:  CommonTabPage();                 break;
                case 1:  CachedDataTabPage();             break;
                case 2:  ModelSettingsTabPage();          break;
                case 3:  ViewSettingsTabPage();           break;
                case 4:  ViewCommonGameSettingsTabPage(); break;
                default: throw new SwitchCaseNotImplementedException(m_TabPage);
            }
        }

        private void CommonTabPage()
        {
            EditorUtilsEx.ScrollViewZone(ref m_CommonScrollPos, () =>
            {
                if (Application.isPlaying)
                    GUILayout.Label($"Target FPS: {Application.targetFrameRate}");
                GUI.enabled = Application.isPlaying;
                if (!GUI.enabled)
                    GUILayout.Label("Available only in play mode:");
                if (GUI.enabled)
                    GUILayout.Space(10);
                EditorUtilsEx.HorizontalZone(() =>
                {
                    EditorUtilsEx.GuiButtonAction("Enable Daily Bonus", EnableDailyBonus);
                    GUILayout.Label("Day:");
                    m_DailyBonusIndex = EditorGUILayout.Popup(
                        m_DailyBonusIndex, new[] { "1", "2", "3", "4", "5", "6", "7" });
                });
                EditorUtilsEx.HorizontalZone(() =>
                {
                    if (GUILayout.Button("Set Game Id:"))
                        SaveUtils.PutValue(SaveKeysCommon.GameId, m_GameId);
                    m_GameId = EditorGUILayout.IntField(m_GameId);
                });
                EditorUtilsEx.HorizontalLine(Color.gray);
                GUI.enabled = true;
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
                EditorUtilsEx.GuiButtonAction("Delete all settings", DeleteAllSettings);
                EditorUtilsEx.GuiButtonAction("Get ready to commit", GetReadyToCommit);
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
                foreach (var scenePath in sceneGuids.Select(AssetDatabase.GUIDToAssetPath))
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
            UpdateQuality();
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
                    if (skVal.Value == "empty")
                        GUI.contentColor = Color.yellow;
                    if (skVal.Value == "not exist")
                        GUI.contentColor = Color.red;
                    GUILayout.Label(skVal.Value);
                    GUI.contentColor = defCol;
                    GUILayout.EndHorizontal();
                }
            });
        }

        private void ModelSettingsTabPage()
        {
            var settings = new PrefabSetManager(new AssetBundleManagerFake()).GetObject<ModelSettings>(
                "model_settings", "model_settings");
            SettingsTabPageCore(settings, typeof(ModelSettings));
        }
    
        private void ViewSettingsTabPage()
        {
            var settings = new PrefabSetManager(new AssetBundleManagerFake()).GetObject<ViewSettings>(
                "model_settings", "view_settings");
            SettingsTabPageCore(settings, typeof(ViewSettings));
        }

        private void ViewCommonGameSettingsTabPage()
        {
            var settings = new PrefabSetManager(new AssetBundleManagerFake()).GetObject<CommonGameSettings>(
                "model_settings", "common_game_settings");
            SettingsTabPageCore(settings, typeof(CommonGameSettings));
        }

        private void SettingsTabPageCore(Object _Settings, Type _Type)
        {
            var fieldInfos = _Type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var serObj = new SerializedObject(_Settings);
            EditorUtilsEx.ScrollViewZone(ref m_ViewSettingsScrollPos, () =>
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
            });
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

        private void UpdateQuality()
        {
            if (m_Quality == -1)
                m_Quality = SaveUtils.GetValue(SaveKeysCommon.GoodQuality) ? 1 : 0;
            if (m_Quality != m_QualityCheck)
                SaveUtils.PutValue(SaveKeysCommon.GoodQuality, m_Quality != 0);
            m_QualityCheck = m_Quality;
        }

        private void EnableDailyBonus()
        {
            SaveUtils.PutValue(SaveKeysRmazor.DailyBonusLastDate, DateTime.Now.Date.AddDays(-1));
            SaveUtils.PutValue(SaveKeysRmazor.DailyBonusLastClickedDay, m_DailyBonusIndex);
        }

        private void CreateTestUsers(int _Count)
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

        private static void GetReadyToCommit()
        {
            string[] files =
            {
                @"Assets\Materials\CircleTransparentTransition.mat",
                @"Assets\Materials\MainMenuBackground.mat"
            };
            foreach (var file in files)
            {
                GitUtils.RunGitCommand($"reset -- {file}");
                GitUtils.RunGitCommand($"checkout -- {file}");
            }
            AssetDatabase.SaveAssets();
        
            BuildSettingsUtils.AddDefaultScenesToBuild();
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

        private void DeleteAllSettings()
        {
            PlayerPrefs.DeleteAll();
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