using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Constants;
using DI.Extensions;
using Entities;
using Exceptions;
using GameHelpers;
using Games.RazorMaze;
using Network;
using Network.Packets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Utils.Editor;
using Object = UnityEngine.Object;

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
    private Vector2 m_ModelSettingsScrollPos;
    private Vector2 m_ViewSettingsScrollPos;

    [MenuItem("Tools/Helper", false, 0)]
    public static void ShowWindow()
    {
        GetWindow<EditorHelper>("Helper");
    }
    
    [MenuItem("Tools/Profiler",false, 3)]
    public static void ShowProfilerWindow()
    {
        Type tProfiler = typeof(Editor).Assembly.GetType("UnityEditor.ProfilerWindow");
        GetWindow(tProfiler, false);
    }
    
#if UNITY_ANDROID
    
    [MenuItem("Tools/Android Logcat",false, 4)]
    public static void ShowAndroidLogcatWindow()
    {
        var tLogcat = typeof(Unity.Android.Logcat.ColumnData).Assembly.GetType("Unity.Android.Logcat.AndroidLogcatConsoleWindow");
        var mInfoShow = tLogcat?.GetMethod("ShowWindow",
            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        mInfoShow?.Invoke(null, null);
    }
#endif

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
                    SaveUtils.PutValue(SaveKeys.GameId, m_GameId);
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
            var sceneGuids = AssetDatabase.FindAssets (
                "l:Scene t:Scene", new[] {"Assets\\Scenes" });
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
        var settings = PrefabUtilsEx.GetObject<ModelSettings>(
            "model_settings", "model_settings");
        var type = typeof(ModelSettings);
        var fieldInfos = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        SettingsTabPageCore(settings, fieldInfos.ToList());
    }
    
    private void ViewSettingsTabPage()
    {
        var settings = PrefabUtilsEx.GetObject<ViewSettings>(
            "model_settings", "view_settings");
        var type = typeof(ViewSettings);
        var fieldInfos = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        SettingsTabPageCore(settings, fieldInfos.ToList());
    }

    private void ViewCommonGameSettingsTabPage()
    {
        var settings = PrefabUtilsEx.GetObject<CommonGameSettings>(
            "model_settings", "common_game_settings");
        var type = typeof(CommonGameSettings);
        var fieldInfos = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        SettingsTabPageCore(settings, fieldInfos.ToList());
    }

    private void SettingsTabPageCore(Object _Settings, List<FieldInfo> _FieldInfos)
    {
        var serObj = new SerializedObject(_Settings);
        EditorUtilsEx.ScrollViewZone(ref m_ViewSettingsScrollPos, () =>
        {
            foreach (var fieldInfo in _FieldInfos)
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
            m_DebugServerUrl = SaveUtils.GetValue(SaveKeys.ServerUrl);
        if (m_DebugServerUrl != m_TestUrlCheck || _Forced)
            SaveUtils.PutValue(SaveKeys.ServerUrl, m_DebugServerUrl);
        m_TestUrlCheck = m_DebugServerUrl;
    }

    private void UpdateGameId()
    {
        if (m_GameId == -1)
            m_GameId = SaveUtils.GetValue(SaveKeys.GameId);
        if (m_GameId != m_GameIdCheck)
            SaveUtils.PutValue(SaveKeys.GameId, m_GameId);
        m_GameIdCheck = m_GameId;
    }

    private void UpdateQuality()
    {
        if (m_Quality == -1)
            m_Quality = SaveUtils.GetValue(SaveKeys.GoodQuality) ? 1 : 0;
        if (m_Quality != m_QualityCheck)
            SaveUtils.PutValue(SaveKeys.GoodQuality, m_Quality != 0);
        m_QualityCheck = m_Quality;
    }

    private void EnableDailyBonus()
    {
        SaveUtils.PutValue(SaveKeys.DailyBonusLastDate, DateTime.Now.Date.AddDays(-1));
        SaveUtils.PutValue(SaveKeys.DailyBonusLastClickedDay, m_DailyBonusIndex);
    }

    private void CreateTestUsers(int _Count)
    {
        CommonData.Testing = true;
        GameClient.Instance.Init();
        int gameId = 1;
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
                    int accId = packet.Response.Id;
                    new GameDataField(100, accId, 
                        GameClientUtils.GameId, DataFieldIds.Money).Save();
                    Dbg.Log("All test users were created successfully");
                })
                .OnFail(() =>
                {
                    Dbg.LogError($"Creating test user #{ii + 1} of {_Count} failed");
                    Dbg.LogError(packet.Response);
                });
            GameClient.Instance.Send(packet);
        }
    }

    private static void DeleteTestUsers()
    {
        CommonData.Testing = true;
        GameClient.Instance.Init();
        IPacket packet = new DeleteTestUsersPacket();
        packet.OnSuccess(() =>
            {
                Dbg.Log("All test users deleted");
            })
            .OnFail(() => Dbg.Log($"Failed to delete test users: {packet.ErrorMessage}"));
        GameClient.Instance.Send(packet);
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
            {"Last connection succeeded", SaveUtils.GetValue(SaveKeys.LastDbConnectionSuccess).ToString()},
            {"Login", SaveUtils.GetValue(SaveKeys.Login) ?? "not exist"},
            {"Password hash", SaveUtils.GetValue(SaveKeys.PasswordHash) ?? "not exist"},
            {"Account id", GameClientUtils.AccountId.ToString()},
            {"Game id", SaveUtils.GetValue(SaveKeys.GameId).ToString()},
            {"First curr.", GetGameFieldCached(DataFieldIds.Money)},
        };

    private static string GetGameFieldCached(ushort _FieldId)
    {
        int? accountId = SaveUtils.GetValue(SaveKeys.AccountId);
        int gameId = SaveUtils.GetValue(SaveKeys.GameId);
        var field = SaveUtils.GetValue(
            SaveKeys.GameDataFieldValue(accountId ?? GameClientUtils.DefaultAccountId, gameId, _FieldId));
        return DataFieldValueString(field);
    }

    private static string DataFieldValueString(DataFieldBase _DataField)
    {
        if (_DataField == null)
            return "not exist";
        string value = _DataField.ToString();
        if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
            return "empty";
        return value;
    }
}