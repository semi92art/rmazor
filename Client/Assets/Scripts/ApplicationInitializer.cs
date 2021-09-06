using Constants;
using Controllers;
using Entities;
using Extensions;
using Managers;
using Network;
using Ticker;
using UI;
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DebugConsole.DebugConsoleView.Instance.Init();
#endif
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(1);
    }
    
    private void OnSceneLoaded(Scene _Scene, LoadSceneMode _)
    {
        Ticker.Clear();
        TimeOrLifesEndedPanel.TimesPanelCalled = 0;
        
        bool onStart = _prevScene.EqualsIgnoreCase(SceneNames.Preload);
                
        if (onStart)
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
                
        if (_Scene.name.EqualsIgnoreCase(SceneNames.Main))
        {
            if (onStart)
                LocalizationManager.Instance.Init();
        }

        _prevScene = _Scene.name;
    }
    
    #endregion
}