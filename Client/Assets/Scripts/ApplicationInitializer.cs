using Constants;
using DI.Extensions;
using GameHelpers;
using Managers;
using Network;
using Ticker;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Zenject;

public class ApplicationInitializer : MonoBehaviour
{
    #region nonpublic members

    private static GameObject _debugReporter; 
    
    #endregion
    
    #region inject
    
    private IGameTicker GameTicker { get; set; }
    private IAdsManager AdsManager { get; set; }
    private IAnalyticsManager AnalyticsManager { get; set; }
    private ILocalizationManager LocalizationManager { get; set; }

    [Inject] 
    public void Inject(
        IGameTicker _GameTicker,
        IAdsManager _AdsManager,
        IAnalyticsManager _AnalyticsManager,
        ILocalizationManager _LocalizationManager)
    {
        GameTicker = _GameTicker;
        AdsManager = _AdsManager;
        AnalyticsManager = _AnalyticsManager;
        LocalizationManager = _LocalizationManager;
    }


    #endregion
    
    #region engine methods
    
    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        DataFieldsMigrator.InitDefaultDataFieldValues();
        InitGameManagers();
        GameTicker.ClearRegisteredObjects();
        SceneManager.LoadScene(SceneNames.Main);
    }
    
    private void OnSceneLoaded(Scene _Scene, LoadSceneMode _)
    {
        
        if (_Scene.name.EqualsIgnoreCase(SceneNames.Main))
        {
            GameClientUtils.GameId = 1; // если игра будет только одна, то и париться с GameId нет смысла
        }
    }

    #endregion
    
    #region nonpublic methods
    
    private void InitGameManagers()
    {
        GameClient.Instance.Init();
        AdsManager.Init();
        AnalyticsManager.Init();
        LocalizationManager.Init();
    }


    #endregion
}