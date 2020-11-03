using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utils;

public class EditorHelper : EditorWindow
{
    private int m_DailyBonusIndex;
    private int m_Gold;
    private int m_Diamonds;
    private readonly string[] m_DailyBonusOptions = { "1", "2", "3", "4", "5", "6", "7" };
    
    [MenuItem("Tools/Helper")]
    public static void ShowWindow()
    {
        GetWindow<EditorHelper>("Helper");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Enable Daily Bonus"))
            EnableDailyBonus();
        GUILayout.Label("Day:");
        m_DailyBonusIndex = EditorGUILayout.Popup(m_DailyBonusIndex, m_DailyBonusOptions);

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Set Money"))
            SetMoney(m_Gold, m_Diamonds);
        GUILayout.Label("Gold:");
        m_Gold = EditorGUILayout.IntField(m_Gold);
        GUILayout.Label("Diamonds:");
        m_Diamonds = EditorGUILayout.IntField(m_Diamonds);
        
        
        GUILayout.EndHorizontal();
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
}
