﻿using System.Collections.Generic;
using Constants;
using Entities;
using Managers;
using Network;
using Network.PacketArgs;
using Network.Packets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

public class EditorHelper : EditorWindow
{
    private int m_DailyBonusIndex;
    private Dictionary<MoneyType, long> m_Money = new Dictionary<MoneyType, long>();
    private int m_TestUsersNum;
    private bool m_IsGuest;
    private string m_TestUrl;
    private string m_TestUrlCheck;
    private int m_GameId;
    private int m_GameIdCheck;
    private int m_Level = 1;
    private readonly string[] m_DailyBonusOptions = { "1", "2", "3", "4", "5", "6", "7" };

    [MenuItem("Tools/Helper")]
    public static void ShowWindow()
    {
        GetWindow<EditorHelper>("Helper");
    }

    private void OnGUI()
    {
        GUI.enabled = Application.isPlaying;
        if (!GUI.enabled)
            GUILayout.Label("Available only in play mode:");
        if (GUI.enabled)
            GUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Enable Daily Bonus"))
            EnableDailyBonus();
        GUILayout.Label("Day:");
        m_DailyBonusIndex = EditorGUILayout.Popup(m_DailyBonusIndex, m_DailyBonusOptions);
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
            if (GUILayout.Button("Get From Bank"))
                GetMoneyFromBank();
            if (GUILayout.Button("Set Money"))
                SetMoney();
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Start Level:"))
            LevelLoader.LoadLevel(m_Level);
        m_Level = EditorGUILayout.IntField(m_Level);
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Pause game"))
            Time.timeScale = 0;
        if (GUILayout.Button("Continue game"))
            Time.timeScale = 1;
        GUILayout.EndHorizontal();
        
        EditorUtils.DrawUiLine(Color.gray);
        GUI.enabled = true;

        if (GUILayout.Button("Print Common Info"))
            PrintCommonInfo();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Create test users"))
            CreateTestUsers(m_TestUsersNum);
        GUILayout.Label("is guest: ");
        m_IsGuest = EditorGUILayout.Toggle(m_IsGuest);
        GUILayout.Label("count: ");
        m_TestUsersNum = EditorGUILayout.IntField(m_TestUsersNum);
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("Delete all test users"))
            DeleteAllTestUsers();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Set Game Id:"))
            SaveUtils.PutValue(SaveKey.GameId, m_GameId);
        m_GameId = EditorGUILayout.IntField(m_GameId);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Debug Server Url:");
        m_TestUrl = EditorGUILayout.TextField(m_TestUrl);
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("Set Default Url"))
            m_TestUrl = @"http://77.37.152.15:7000";
        if (GUILayout.Button("Delete All Settings"))
            PlayerPrefs.DeleteAll();
        if (GUILayout.Button("Set Default Materal Props"))
            SetDefaultMaterialProps();
        EditorUtils.DrawUiLine(Color.gray);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(SceneNames.Preload))
            LoadScene($"Assets/Scenes/{SceneNames.Preload}.unity");
        if (GUILayout.Button(SceneNames.Main))
            LoadScene($"Assets/Scenes/{SceneNames.Main}.unity");
        if (GUILayout.Button(SceneNames.Level))
            LoadScene($"Assets/Scenes/{SceneNames.Level}.unity");
        GUILayout.EndHorizontal();
        
        UpdateTestUrl();
        UpdateGameId();
    }

    private void UpdateTestUrl()
    {
        if (m_TestUrl != m_TestUrlCheck)
            SaveUtils.PutValue(SaveKey.DebugServerUrl, m_TestUrl);
        if (string.IsNullOrEmpty(m_TestUrl))
            m_TestUrl = SaveUtils.GetValue<string>(SaveKey.DebugServerUrl);
        m_TestUrlCheck = m_TestUrl;
    }

    private void UpdateGameId()
    {
        if (m_GameId != m_GameIdCheck)
            SaveUtils.PutValue(SaveKey.GameId, m_GameId);
        if (m_GameId == 0)
            m_GameId = SaveUtils.GetValue<int>(SaveKey.GameId);
        m_GameIdCheck = m_GameId;
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

    private void PrintCommonInfo()
    {
        Debug.Log($"Account Id: {GameClient.Instance.AccountId}");
        Debug.Log($"Device Id: {GameClient.Instance.DeviceId}");
    }

    private void CreateTestUsers(int _Count)
    {
        System.Func<int, string> resultTextBegin = _I => $"Creating test user #{_I + 1} of {_Count}";
        GameClient.Instance.Init(true);
        int gameId = 1;
        System.Random randGen = new System.Random();
        for (int i = 0; i < _Count; i++)
        {
            var packet = new RegisterUserPacket(
                new RegisterUserUserPacketRequestArgs
                {
                    Name = m_IsGuest ? string.Empty : $"test_{Utility.GetUniqueId()}",
                    PasswordHash = Utility.GetMD5Hash("1"),
                    DeviceId = m_IsGuest ? $"test_{Utility.GetUniqueId()}" : string.Empty,
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
                                Points = randGen.Next(0, 500),
                                Type = 1
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

    private void DeleteAllTestUsers()
    {
        IPacket packet = new DeleteTestUsersPacket();
        packet.OnSuccess(() =>
            {
                Debug.Log("All test users deleted");
            })
            .OnFail(() => Debug.Log($"Failed to delete test users: {packet.ErrorMessage}"));
        GameClient.Instance.Init(true);
        GameClient.Instance.Send(packet);
    }

    private void SetDefaultMaterialProps()
    {
        string matPath = @"Assets\Materials\CircleTransparentTransition.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        mat.SetFloat(CircleTransparentTransitionRenderer.AlphaCoeff, -1);
    }

    private void LoadScene(string _Name)
    {
        if (Application.isPlaying)
            SceneManager.LoadScene(_Name);
        else
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(_Name);    
        }
        
    }
}


