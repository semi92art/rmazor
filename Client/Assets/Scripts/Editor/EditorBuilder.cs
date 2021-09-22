using System.IO;
using System.Linq;
using Constants;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Utils;

public class EditorBuilder : EditorWindow
{
    #region nonpublic members
    
    private int m_GamePopupIdx;
    private int m_PlatformIdx;
    private GameInfo[] m_GameInfos;
    private string[] m_GameTitles;
    private readonly string[] m_PlatformNames = {"Android", "iOS"};
    private bool m_BuildBundles;

    #endregion
    

    [MenuItem("Tools/Builder", false, 2)]
    public static void ShowWindow()
    {
        GetWindow<EditorBuilder>("Builder");
    }

    private void OnEnable()
    {
        m_GameInfos = GameInfo.Infos.Where(_Info => _Info.Available).ToArray();
        m_GameTitles = m_GameInfos
            .Select(_Info => $"{_Info.GameId}: {_Info.Title}")
            .ToArray();
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Game:");
        m_GamePopupIdx = EditorGUILayout.Popup(m_PlatformIdx, m_GameTitles);
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Platform:");
        m_PlatformIdx = EditorGUILayout.Popup(m_PlatformIdx, m_PlatformNames);
        GUILayout.EndHorizontal();
        
        m_BuildBundles = EditorGUILayout.Toggle("Build Asset Bundles", m_BuildBundles);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Development Build"))
            Build(true);
        if (GUILayout.Button("Release Build"))
            Build(false);
        GUILayout.EndHorizontal();
    }

    #region nonpublic methods

    private void Build(bool _Development)
    {
        bool isAndroid = m_PlatformIdx == 0;
        Dbg.Log($"Starting {(_Development ? "Development" : "Release")} Build." +
                $" Game: {m_GameInfos[m_GamePopupIdx].Title};" +
                $" Platform: {m_PlatformNames[m_PlatformIdx]}");
        var buildTarget = isAndroid ? BuildTargetGroup.Android : BuildTargetGroup.iOS;
        string defSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);
        foreach (var info in m_GameInfos)
            defSymbols = defSymbols.Replace($";GAME_{info.GameId}", string.Empty);
        defSymbols = defSymbols + $";GAME_{m_GameInfos[m_GamePopupIdx].GameId}";
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, defSymbols);
        if (m_BuildBundles)
            BuildAssetBundles.BuildAllAssetBundles();
        
        var bpo = new BuildPlayerOptions();
        bpo.scenes = new[] {SceneNames.Preload, SceneNames.Main, SceneNames.Level}
            .Select(_Name => $"Assets/Scenes/{_Name}.unity").ToArray();
        
        string folder = $"Builds/{m_PlatformNames[m_PlatformIdx]}";
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        if (isAndroid)
            bpo.locationPathName = $"{folder}/{(_Development ? "development" : "release")}_{Application.version}.apk";
        bpo.target = isAndroid ? BuildTarget.Android : BuildTarget.iOS;
        bpo.options = _Development
            ? BuildOptions.Development
            : BuildOptions.CompressWithLz4HC | BuildOptions.StrictMode;
        BuildReport report = BuildPipeline.BuildPlayer(bpo);
        BuildSummary summary = report.summary;
        
        switch (summary.result)
        {
            case BuildResult.Succeeded:
                Dbg.Log("Build succeeded");
                break;
            case BuildResult.Failed:
                Dbg.LogError("Build failed");
                break;
        }
    }

    #endregion
}

