using Constants;
using Entities;
using Extensions;
using Games.RazorMaze;
using Managers;
using Network;
using Ticker;
using UI.Panels;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Zenject;

public class ApplicationInitializer : MonoBehaviour
{
    #region inject
    
    private ITicker Ticker { get; set; }

    [Inject] public void Inject(ITicker _Ticker) => Ticker = _Ticker;
    
    #endregion
    
    #region engine methods

    private static string _prevScene = SceneNames.Preload;
    
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        GameClient.Instance.Init();
        AdsManager.Instance.Init();
        AnalyticsManager.Instance.Init();
        AssetBundleManager.Instance.Init();
        InitDebugConsole();
        SceneManager.sceneLoaded += OnSceneLoaded;
        CommonUtils.LoadSceneCorrectly(Ticker, SceneNames.Main);
    }
    
    private void OnSceneLoaded(Scene _Scene, LoadSceneMode _)
    {
        TimeOrLifesEndedPanel.TimesPanelCalled = 0;
        
        bool onStart = _prevScene.EqualsIgnoreCase(SceneNames.Preload);
                
        if (onStart)
            InitDebugReporter();
        
        if (_Scene.name.EqualsIgnoreCase(SceneNames.Main))
        {
            GameClientUtils.GameId = 1;
            if (onStart)
                LocalizationManager.Instance.Init();
        }
        
        _prevScene = _Scene.name;
    }

    #endregion
    
    #region nonpublic methods

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
                    UiManager.Instance.DebugReporter = GameHelpers.PrefabUtilsEx.InitPrefab(
                        null,
                        "debug_console",
                        "reporter");
                    UiManager.Instance.DebugReporter.SetActive(debugOn);
#endif
#endif
    }

    #endregion
}