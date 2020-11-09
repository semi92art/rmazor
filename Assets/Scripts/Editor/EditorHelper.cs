using System.Collections.Generic;
using Entities;
using Managers;
using Network;
using Network.PacketArgs;
using Network.Packets;
using UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Utils;

public class EditorHelper : EditorWindow
{
    private int m_DailyBonusIndex;
    private int m_Gold;
    private int m_Diamonds;
    private int m_TestUsersNum;
    private bool m_IsGuest;
    private string m_TestUrl;
    private string m_TestUrlCheck;
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
            GUILayout.Label("available only in play mode:");
        if (GUI.enabled)
            GUILayout.Space(10);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Enable Daily Bonus"))
            EnableDailyBonus();
        GUILayout.Label("Day:");
        m_DailyBonusIndex = EditorGUILayout.Popup(m_DailyBonusIndex, m_DailyBonusOptions);

        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Set Money"))
            SetMoney(m_Gold, m_Diamonds);
        GUILayout.Label("gold: ");
        m_Gold = EditorGUILayout.IntField(m_Gold);
        GUILayout.Label("diamonds: ");
        m_Diamonds = EditorGUILayout.IntField(m_Diamonds);

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
        GUILayout.Label("Debug Server Url:");
        m_TestUrl = EditorGUILayout.TextField(m_TestUrl);
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Set Default Url"))
            m_TestUrl = @"http://77.37.152.15:7000";
        if (GUILayout.Button("Delete All Settings"))
            PlayerPrefs.DeleteAll();
    
        EditorUtils.DrawUiLine(Color.gray);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("_preload"))
            EditorSceneManager.OpenScene("Assets/Scenes/_preload.unity");
        if (GUILayout.Button("Main"))
            EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        if (GUILayout.Button("Level"))
            EditorSceneManager.OpenScene("Assets/Scenes/Level.unity");
        GUILayout.EndHorizontal();
    

        UpdateTestUrl();
    }

    private void UpdateTestUrl()
    {
        if (m_TestUrl != m_TestUrlCheck)
            SaveUtils.PutValue(SaveKey.DebugServerUrl, m_TestUrl);
        if (string.IsNullOrEmpty(m_TestUrl))
            m_TestUrl = SaveUtils.GetValue<string>(SaveKey.DebugServerUrl);
        m_TestUrlCheck = m_TestUrl;
    }

    private void EnableDailyBonus()
    {
        SaveUtils.PutValue(SaveKey.DailyBonusLastDate, new System.DateTime(2000, 1, 1));
        SaveUtils.PutValue(SaveKey.DailyBonusLastItemClickedDay, m_DailyBonusIndex);
        SaveUtils.PutValue(SaveKey.DailyBonusOnDebug, true);
    }

    private void SetMoney(int _Gold, int _Diamonds)
    {
        var money = new Dictionary<MoneyType, int>
        {
            {MoneyType.Gold, _Gold},
            {MoneyType.Diamonds, _Diamonds}
        };
        MoneyManager.Instance.SetMoney(money);
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
        int cCount = Countries.Keys.Count;
        for (int i = 0; i < _Count; i++)
        {
            var packet = new RegisterUserPacket(
                new RegisterUserUserPacketRequestArgs
                {
                    Name = m_IsGuest ? string.Empty : $"test_{Utility.GetUniqueId()}",
                    PasswordHash = Utility.GetMD5Hash("1"),
                    DeviceId = m_IsGuest ? $"test_{Utility.GetUniqueId()}" : string.Empty,
                    GameId = gameId,
                    CountryKey = Countries.Keys[randGen.Next(0, cCount)]
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
}


