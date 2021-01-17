using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Utils;
using System.IO;

public class LocalizationHelper : EditorWindow
{
    #region types

    private class KeyValues
    {
        public Dictionary<Language, string> Values { get; } = new Dictionary<Language, string>();
    }
    
    #endregion
    
    #region nonpublic members
    
    private Dictionary<string, KeyValues> m_LocalizedDict;
    private Dictionary<Language, TextAsset> m_Assets;
    private string m_NewKey;
    private Vector2 m_ScrollPos;
    private string m_ErrorText;
    
    #endregion
    
    [MenuItem("Tools/Localization Helper", false, 1)]
    public static void ShowWindow()
    {
        var window = GetWindow<LocalizationHelper>("Localization Helper");
        window.minSize = new Vector2(300, 200);
        window.m_NewKey = "new_key";
    }

    private void OnEnable()
    {
        LoadResources();
    }

    private void OnGUI()
    {
        m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos, false, false);
        var boldStyle = new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 13
        };
        
        // languages row
        var languages = GetLanguages();
        var firstRow = new List<string> {"Key"}
            .Concat(languages.Select(_Lang => _Lang.ToString()))
            .ToList();
        
        GUILayout.BeginHorizontal();
        GUI.enabled = false;
        for (int i = 0; i < firstRow.Count; i++)
        {
            var item = firstRow[i];
            if (i == 0)
                EditorGUILayout.TextField(item, boldStyle, GUILayout.Width(150));
            else
                EditorGUILayout.TextField(item, boldStyle);
        }

        GUILayout.TextField(string.Empty, boldStyle, GUILayout.Width(50));
        GUILayout.EndHorizontal();
        GUI.enabled = true;
        
        EditorUtils.DrawUiLine(Color.gray);
        
        //values rows
        foreach (var kvp in m_LocalizedDict.ToArray())
        {
            GUILayout.BeginHorizontal();
            GUI.enabled = false;
            GUILayout.TextField(kvp.Key, GUILayout.Width(150));
            GUI.enabled = true;
            foreach (var lang in languages)
            {
                m_LocalizedDict[kvp.Key].Values[lang] =
                    EditorGUILayout.TextField(m_LocalizedDict[kvp.Key].Values[lang]);
            }
            
            if (GUILayout.Button("-", GUILayout.Width(50)))
                DeleteKey(kvp.Key);
            
            GUILayout.EndHorizontal();
        }
        
        EditorUtils.DrawUiLine(Color.gray);
        
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save", GUILayout.Height(40)))
            Save();
        if (GUILayout.Button("Discard", GUILayout.Height(40)))
            Discard();
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (!string.IsNullOrEmpty(m_ErrorText))
            GUI.enabled = false;
        if (GUILayout.Button("Add key", GUILayout.Width(100)))
            AddKey(m_NewKey);
        GUI.enabled = true;
        m_NewKey = EditorGUILayout.TextField(m_NewKey, GUILayout.Width(200));
        var defaultContentColor = GUI.contentColor;
        GUI.contentColor = Color.red;
        GUILayout.Label(m_ErrorText, EditorStyles.boldLabel);
        GUI.contentColor = defaultContentColor;
        GUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
        CheckForErrors();
    }

    private void CheckForErrors()
    {
        m_ErrorText = m_LocalizedDict.Keys.Any(_DictKey => _DictKey == m_NewKey) ?
            $@"Localization already contains key ""{m_NewKey}""" : string.Empty;
    }

    private void LoadResources()
    {
        m_LocalizedDict = new Dictionary<string, KeyValues>();
        m_Assets = new Dictionary<Language, TextAsset>();
        var languages = GetLanguages();
        foreach (Language language in languages)
        {
            TextAsset textAsset = ResLoader.GetText($@"texts\{language}");
            m_Assets.Add(language, textAsset);
            var lines = textAsset.text.Split(new[] {"\r\n"}, StringSplitOptions.None)
                .Except(new[] {string.Empty, null});
            foreach (var line in lines)
            {
                var keyAndValue = line.Split(new[] {" = "}, StringSplitOptions.None);
                string key = keyAndValue[0];
                string value = keyAndValue[1];
                if (!m_LocalizedDict.ContainsKey(key))
                    m_LocalizedDict.Add(key, new KeyValues());
                m_LocalizedDict[key].Values.Add(language, value);
            }
        }
    }

    private void AddKey(string _Key)
    {
        if (string.IsNullOrEmpty(_Key))
        {
            Debug.LogError("Key is empty!");
            return;
        }

        m_LocalizedDict.Add(_Key, new KeyValues());
        foreach (var lang in GetLanguages())
            m_LocalizedDict[_Key].Values.Add(lang, string.Empty);
    }

    private void DeleteKey(string _Key)
    {
        m_LocalizedDict.Remove(_Key);
    }

    private void Save()
    {
        var languages = GetLanguages();
        var langKeysDict = new Dictionary<Language, List<string>>();

        foreach (var lang in languages)
            langKeysDict.Add(lang, new List<string>());
        
        foreach (var kvp in m_LocalizedDict.ToArray())
        foreach (var langKey in langKeysDict.Keys.ToArray())
                langKeysDict[langKey].Add($"{kvp.Key} = {kvp.Value.Values[langKey]}\r\n");

        var sb = new System.Text.StringBuilder();
        foreach (var key in m_Assets.Keys.ToArray())
        {
            sb.Clear();
            foreach (var line in langKeysDict[key])
                sb.Append(line);
            File.WriteAllText($@"Assets\Resources\texts\{m_Assets[key].name}.txt", sb.ToString());
        }
        
        Debug.Log("Localization changes saved successfully");
    }

    private void Discard()
    {
        LoadResources();
        Debug.Log("Localization changes were discarded");
    }

    private Language[] GetLanguages()
    {
        return Enum.GetValues(typeof(Language))
            .Cast<Language>().ToArray();
    }
}