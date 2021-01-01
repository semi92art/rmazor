using System;
using System.Collections.Generic;
using Constants;
using Entities;
using Managers;
using Network;
using Network.PacketArgs;
using Network.Packets;
using PygmyMonkey.ColorPalette;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_ANDROID
using System.Reflection;
using Unity.Android.Logcat;
#endif
using UnityEngine.Events;
using Utils;

public class EditorHelper : EditorWindow
{
    private int m_DailyBonusIndex;
    private Dictionary<MoneyType, long> m_Money = new Dictionary<MoneyType, long>();
    private int m_TestUsersNum;
    private bool m_IsGuest;
    private string m_TestUrl;
    private string m_TestUrlCheck;
    private int m_GameId = -1;
    private int m_GameIdCheck;
    private int m_Level = -1;
    private int m_Quality = -1;
    private int m_QualityCheck;

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
        Type tLogcat = typeof(ColumnData).Assembly.GetType("Unity.Android.Logcat.AndroidLogcatConsoleWindow");
        MethodInfo mInfoShow = tLogcat?.GetMethod("ShowWindow",
            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        mInfoShow?.Invoke(null, null);
    }
#endif
    
    [MenuItem("Tools/Color Palette", false, 5)]
    private static void ShowColorPaletteWindow()
    {
        EditorWindow window = ColorPaletteWindow.createWindow<ColorPaletteWindow>("Color Palette");
        window.minSize = new Vector2(280, 500);
    }

    private void OnEnable()
    {
        UpdateTestUrl();
        UpdateGameId();
        UpdateQuality();
    }

    private void OnGUI()
    {
        if (Application.isPlaying)
        {
            GUILayout.Label("Account info:");
            GUILayout.Label($"Account Id: {GameClient.Instance.AccountId}");
            GUILayout.Label($"Device Id: {GameClient.Instance.DeviceId}");
        }
        
        GUI.enabled = Application.isPlaying;
        if (!GUI.enabled)
            GUILayout.Label("Available only in play mode:");
        if (GUI.enabled)
            GUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        GuiButtonAction("Enable Daily Bonus", EnableDailyBonus);
        GUILayout.Label("Day:");
        m_DailyBonusIndex = EditorGUILayout.Popup(
            m_DailyBonusIndex, new[] { "1", "2", "3", "4", "5", "6", "7" });
        GUILayout.EndHorizontal();
        
        if (Application.isPlaying)
        {
            GUILayout.BeginHorizontal();
            var money = m_Money.CloneAlt();
            foreach (var kvp in m_Money)
            {
                GUILayout.Label($"{kvp.Key}:");
                money[kvp.Key] = EditorGUILayout.LongField(money[kvp.Key]);
            }
            m_Money = money;
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GuiButtonAction("Get From Bank", GetMoneyFromBank);
            GuiButtonAction("Set Money", SetMoney);
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();
        GuiButtonAction("Start Level:", LevelLoader.LoadLevel, m_Level);
        m_Level = EditorGUILayout.IntField(m_Level);
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GuiButtonAction("Pause game", PauseGame, true);
        GuiButtonAction("Continue game", PauseGame, false);
        GUILayout.EndHorizontal();
        
        EditorUtils.DrawUiLine(Color.gray);
        GUI.enabled = true;

        GuiButtonAction("Print common info", PrintCommonInfo);

        GUILayout.BeginHorizontal();
        GuiButtonAction("Create test users", CreateTestUsers, m_TestUsersNum);
        GUILayout.Label("is guest: ");
        m_IsGuest = EditorGUILayout.Toggle(m_IsGuest);
        GUILayout.Label("count: ");
        m_TestUsersNum = EditorGUILayout.IntField(m_TestUsersNum);
        GUILayout.EndHorizontal();
        
        GuiButtonAction("Delete test users", DeleteTestUsers);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Set Game Id:"))
            SaveUtils.PutValue(SaveKey.GameId, m_GameId);
        m_GameId = EditorGUILayout.IntField(m_GameId);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Debug Server Url:");
        m_TestUrl = EditorGUILayout.TextField(m_TestUrl);
        GUILayout.EndHorizontal();

        GuiButtonAction("Set default api url", SetDefaultApiUrl);
        GuiButtonAction("Delete all settings", DeleteAllSettings);

        GuiButtonAction("Set Default Material Props", SetDefaultMaterialProps);
        EditorUtils.DrawUiLine(Color.gray);
        
        GUILayout.BeginHorizontal();
        GuiButtonAction(SceneNames.Preload, LoadScene, $"Assets/Scenes/{SceneNames.Preload}.unity");
        GuiButtonAction(SceneNames.Main, LoadScene, $"Assets/Scenes/{SceneNames.Main}.unity");
        GuiButtonAction(SceneNames.Level, LoadScene, $"Assets/Scenes/{SceneNames.Level}.unity");
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Quality:");
        m_Quality = EditorGUILayout.Popup(
            m_Quality, new[] { "Normal", "Good" });
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Fix Firebase Dependencies"))
        {
            // FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(_Task =>
            // {
            //     FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            // });
        }
        
        UpdateTestUrl();
        UpdateGameId();
        UpdateQuality();
    }

    private void UpdateTestUrl(bool _Forced = false)
    {
        if (string.IsNullOrEmpty(m_TestUrl))
            m_TestUrl = SaveUtils.GetValue<string>(SaveKeyDebug.ServerUrl);
        if (m_TestUrl != m_TestUrlCheck || _Forced)
            SaveUtils.PutValue(SaveKeyDebug.ServerUrl, m_TestUrl);
        m_TestUrlCheck = m_TestUrl;
    }

    private void UpdateGameId()
    {
        if (m_GameId == -1)
            m_GameId = SaveUtils.GetValue<int>(SaveKey.GameId);
        if (m_GameId != m_GameIdCheck)
            SaveUtils.PutValue(SaveKey.GameId, m_GameId);
        m_GameIdCheck = m_GameId;
    }

    private void UpdateQuality()
    {
        if (m_Quality == -1)
            m_Quality = SaveUtils.GetValue<bool>(SaveKeyDebug.GoodQuality) ? 1 : 0;
        if (m_Quality != m_QualityCheck)
            SaveUtils.PutValue(SaveKeyDebug.GoodQuality, m_Quality != 0);
        m_QualityCheck = m_Quality;
    }

    private void EnableDailyBonus()
    {
        SaveUtils.PutValue(SaveKey.DailyBonusLastDate, System.DateTime.Now.Date.AddDays(-1));
        SaveUtils.PutValue(SaveKey.DailyBonusLastItemClickedDay, m_DailyBonusIndex);
    }

    private void GetMoneyFromBank()
    {
        var bank = MoneyManager.Instance.GetBank();
        Coroutines.Run(Coroutines.WaitWhile(
            () =>  m_Money = bank.Money,
            () => !bank.Loaded));
    }
    
    private void SetMoney()
    {
        MoneyManager.Instance.SetMoney(m_Money);
    }

    private static void PrintCommonInfo()
    {
        Debug.Log($"Account Id: {GameClient.Instance.AccountId}");
        Debug.Log($"Device Id: {GameClient.Instance.DeviceId}");
    }

    private void CreateTestUsers(int _Count)
    {
        Func<int, string> resultTextBegin = _I => $"Creating test user #{_I + 1} of {_Count}";
        GameClient.Instance.Init(true);
        int gameId = 1;
        System.Random randGen = new System.Random();
        for (int i = 0; i < _Count; i++)
        {
            var packet = new RegisterUserPacket(
                new RegisterUserPacketRequestArgs
                {
                    Name = m_IsGuest ? string.Empty : $"test_{CommonUtils.GetUniqueId()}",
                    PasswordHash = CommonUtils.GetMd5Hash("1"),
                    DeviceId = m_IsGuest ? $"test_{CommonUtils.GetUniqueId()}" : string.Empty,
                    GameId = gameId
                });
            int ii = i;
            packet.OnSuccess(() =>
                {
                    var profPacket = new SetProfilePacket(new SetProfileRequestArgs
                    {
                        AccountId = packet.Response.Id,
                        Gold = randGen.Next(0, 100000),
                        Diamonds = randGen.Next(0, 100)
                    });
                    int iii = ii;
                    profPacket.OnSuccess(() =>
                        {
                            var scPacket = new SetScorePacket(new SetScoreRequestArgs
                            {
                                AccountId = packet.Response.Id,
                                GameId = gameId,
                                LastUpdateTime = System.DateTime.Now,
                                Points = randGen.Next(0, 100),
                                Type = ScoreTypes.MaxScore
                            });

                            int iiii = iii;
                            scPacket.OnSuccess(() =>
                            {
                                Debug.Log($"{resultTextBegin(iiii)} score succeeded");
                            }).OnFail(() => Debug.Log($"{resultTextBegin(iiii)} score failed"));
                            GameClient.Instance.Send(scPacket);
                        })
                        .OnFail(() => Debug.Log($"{resultTextBegin(iii)} profile failed"));
                    GameClient.Instance.Send(profPacket);
                })
                .OnFail(() => Debug.Log($"{resultTextBegin(ii)} failed"));
    
            GameClient.Instance.Send(packet);
        }
    }

    private static void DeleteTestUsers()
    {
        GameClient.Instance.Init(true);
        IPacket packet = new DeleteTestUsersPacket();
        packet.OnSuccess(() =>
            {
                Debug.Log("All test users deleted");
            })
            .OnFail(() => Debug.Log($"Failed to delete test users: {packet.ErrorMessage}"));
        GameClient.Instance.Send(packet);
    }

    private static void SetDefaultMaterialProps()
    {
        string matPath = @"Assets\Materials\CircleTransparentTransition.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        mat.SetFloat(CircleTransparentTransitionRenderer.AlphaCoeff, -1);
        AssetDatabase.SaveAssets();
    }

    private static void LoadScene(string _Name)
    {
        if (Application.isPlaying)
            SceneManager.LoadScene(_Name);
        else
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(_Name);    
        }       
    }
    
    private static void PauseGame(bool _Pause)
    {
        UiTimeProvider.Instance.Pause = _Pause;
        GameTimeProvider.Instance.Pause = _Pause;
    }

    private void SetDefaultApiUrl()
    {
        m_TestUrl = @"http://77.37.152.15:7000";
        UpdateTestUrl(true);
    }

    private void DeleteAllSettings()
    {
        PlayerPrefs.DeleteAll();
        UpdateTestUrl(true);
    }

    private static void GuiButtonAction(string _Name, UnityAction _Action)
    {
        if (GUILayout.Button(_Name))
            _Action?.Invoke();
    }
    
    private static void GuiButtonAction<T>(string _Name, UnityAction<T> _Action, T _Arg)
    {
        if (GUILayout.Button(_Name))
            _Action?.Invoke(_Arg);
    }
}


