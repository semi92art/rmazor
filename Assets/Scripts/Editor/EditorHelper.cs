using UnityEditor;
using UnityEngine;
using Utils;

public class EditorHelper : EditorWindow
{
    private int m_DailyBonusIndex;
    private readonly string[] m_DailyBonusOptions = { "1", "2", "3", "4", "5", "6", "7" };
    
    [MenuItem("Tools/Helper")]
    public static void ShowWindow()
    {
        GetWindow<EditorHelper>("Helper");
    }

    private void OnGUI()
    {
        m_DailyBonusIndex = EditorGUILayout.Popup(m_DailyBonusIndex, m_DailyBonusOptions);
        if (GUILayout.Button("Enable Daily Bonus"))
            EnableDailyBonus();
    }

    private void EnableDailyBonus()
    {
        SaveUtils.PutValue(SaveKey.DailyBonusLastDate, new System.DateTime(2000, 1, 1));
        SaveUtils.PutValue(SaveKey.DailyBonusLastItemClickedDay, m_DailyBonusIndex);
        SaveUtils.PutValue(SaveKey.DailyBonusOnDebug, true);
        
        
    }
}
