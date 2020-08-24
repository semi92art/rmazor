using System;
using UnityEditor;
using UnityEngine;

public class EditorBuilder : EditorWindow
{
    private readonly string[] m_PlatformPopupOptions = { "Android", "iOS" };
    private int m_PlatformPopupIndex;

    [MenuItem("Tools/Builder")]

    public static void ShowWindow()
    {
        GetWindow<EditorBuilder>("EditorBuilder");
    }

    private void OnGUI()
    {
        //Platform (Android/iOS) selection

        this.m_PlatformPopupIndex = EditorGUILayout.Popup(this.m_PlatformPopupIndex, this.m_PlatformPopupOptions);
        if (GUILayout.Button("Set"))
            this.PlatformSelector();
        if (GUILayout.Button("Developer Build"))
            this.LoadDeveloperBuild();
        if (GUILayout.Button("Release Build"))
            this.LoadReleaseBuild();
    }

    private void PlatformSelector()
    {
        switch (this.m_PlatformPopupIndex)
        {
            case 0:
                Debug.Log("Switching platform to: Android");
                break;
            case 1:
                Debug.Log("Switching platform to: iOS");
                break;
        }
    }

    private void LoadDeveloperBuild()
    {
        Debug.Log("Loading Developer Build");
    }

    private void LoadReleaseBuild()
    {
        Debug.Log("Loading Release Build");
    }
}
