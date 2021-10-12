using Constants;
using DI.Extensions;
using Entities;
using Managers;
using Network;
using Ticker;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Zenject;

public class ApplicationInitializer : MonoBehaviour
{
    #region inject
    
    private IGameTicker GameTicker { get; set; }

    [Inject] public void Inject(IGameTicker _GameTicker) => GameTicker = _GameTicker;
    
    #endregion
    
    #region engine methods

    private static string _prevScene = SceneNames.Preload;
    
    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        InitDebugConsole();
        InitGameManagers();
        
        GameTicker.ClearRegisteredObjects();
        SceneManager.LoadScene(SceneNames.Main);
    }
    
    private void OnSceneLoaded(Scene _Scene, LoadSceneMode _)
    {
        bool mainSceneLoadedFirstTime = _prevScene.EqualsIgnoreCase(SceneNames.Preload);
        if (mainSceneLoadedFirstTime)
            InitDebugReporter();
        if (_Scene.name.EqualsIgnoreCase(SceneNames.Main))
        {
            GameClientUtils.GameId = 1; // если игра будет только одна, то и париться с GameId нет смысла
            if (mainSceneLoadedFirstTime)
                LocalizationManager.Instance.Init();
        }
        
        _prevScene = _Scene.name;
    }

    #endregion
    
    #region nonpublic methods
    
    private static void InitGameManagers()
    {
        var managers = new IInit[]
        {
            GameClient.Instance,
            AdsManager.Instance,
            AnalyticsManager.Instance,
            AssetBundleManager.Instance
        };
        foreach (var manager in managers)
            manager.Init();
    }

    private static void InitDebugConsole()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DebugConsole.DebugConsoleView.Instance.Init();
#endif
    }
    
    private static void InitDebugReporter()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        bool debugOn = SaveUtils.GetValue<bool>(SaveKeyDebug.DebugUtilsOn);
        SaveUtils.PutValue(SaveKeyDebug.DebugUtilsOn, debugOn);
        DebugConsole.DebugConsoleView.Instance.SetGoActive(debugOn);
#if !UNITY_EDITOR && DEVELOPMENT_BUILD
        UI.Managers.UiManager.Instance.DebugReporter = GameHelpers.PrefabUtilsEx.InitPrefab(
            null,
            "debug_console",
            "reporter");
        UI.Managers.UiManager.Instance.DebugReporter.SetActive(debugOn);
#endif
#endif
    }

    #endregion
}