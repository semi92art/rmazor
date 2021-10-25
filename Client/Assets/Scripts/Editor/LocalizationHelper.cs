using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Utils;
using Utils.Editor;

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
        EditorUtilsEx.ScrollViewZone(ref m_ScrollPos, () =>
        {
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
        
        EditorUtilsEx.HorizontalZone(() =>
        {
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
        });

        GUI.enabled = true;
        
        EditorUtilsEx.HorizontalLine(Color.gray);
        
        //values rows
        foreach (var kvp in m_LocalizedDict.ToArray())
        {
            EditorUtilsEx.HorizontalZone(() =>
            {
                GUI.enabled = false;
                GUILayout.TextField(kvp.Key, GUILayout.Width(150));
                GUI.enabled = true;
                foreach (var lang in languages)
                {
                    string value = m_LocalizedDict[kvp.Key].Values[lang];
                    EditorUtilsEx.GUIColorZone(value == "[Empty]" ? new Color(0.51f, 0.13f, 0.12f) : GUI.color, () =>
                    {
                        m_LocalizedDict[kvp.Key].Values[lang] = EditorGUILayout.TextField(value);    
                    });
                }
                if (GUILayout.Button("-", GUILayout.Width(50)))
                    DeleteKey(kvp.Key);
            });
        }
        EditorUtilsEx.HorizontalLine(Color.gray);
        GUILayout.Space(10);
        EditorUtilsEx.HorizontalZone(() =>
        {
            if (GUILayout.Button("Save", GUILayout.Height(40)))
                Save();
            if (GUILayout.Button("Discard", GUILayout.Height(40)))
                Discard();
        });
            
        GUILayout.Space(10);
        EditorUtilsEx.HorizontalZone(() =>
        {
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
        });
        CheckForErrors();
        });
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
            Dbg.LogError("Key is empty!");
            return;
        }
        m_LocalizedDict.Add(_Key, new KeyValues());
        foreach (var lang in GetLanguages())
            m_LocalizedDict[_Key].Values.Add(lang, "[Empty]");
    }

    private void DeleteKey(string _Key)
    {
        m_LocalizedDict.Remove(_Key);
    }

    private void Save()
    {
        var languages = GetLanguages();
        var langKeysDict = languages
            .ToDictionary(_Lang => _Lang, _Lang => new List<string>());

        foreach (var kvp in m_LocalizedDict.ToArray())
        foreach (var langKey in langKeysDict.Keys.ToArray())
                langKeysDict[langKey].Add($"{kvp.Key} = {kvp.Value.Values[langKey]}\r\n");

        var sb = new StringBuilder();
        foreach (var key in m_Assets.Keys.ToArray())
        {
            sb.Clear();
            foreach (var line in langKeysDict[key])
                sb.Append(line);
            File.WriteAllText($@"Assets\Resources\texts\{m_Assets[key].name}.txt", sb.ToString());
        }

        AssetDatabase.SaveAssets();
        Dbg.Log("Localization changes saved successfully");
    }

    private void Discard()
    {
        LoadResources();
        Dbg.Log("Localization changes were discarded");
    }

    private Language[] GetLanguages()
    {
        return Enum.GetValues(typeof(Language))
            .Cast<Language>().ToArray();
    }
}