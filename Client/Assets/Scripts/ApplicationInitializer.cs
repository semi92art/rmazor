using Constants;
using Controllers;
using DI.Extensions;
using Entities;
using GameHelpers;
using Games.RazorMaze.Controllers;
using Managers;
using Mono_Installers;
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

    private IViewGameTicker      ViewGameTicker      { get; set; }
    private IModelGameTicker     ModelGameTicker     { get; set; }
    private IUITicker            UITicker            { get; set; }
    private IAdsManager          AdsManager          { get; set; }
    private IAnalyticsManager    AnalyticsManager    { get; set; }
    private ILocalizationManager LocalizationManager { get; set; }
    private ILevelsLoader        LevelsLoader        { get; set; }
    private IScoreManager        ScoreManager        { get; set; }
    private IHapticsManager      HapticsManager      { get; set; }

    [Inject] 
    public void Inject(
        IViewGameTicker _ViewGameTicker,
        IModelGameTicker _ModelGameTicker,
        IUITicker _UITicker,
        IAdsManager _AdsManager,
        IAnalyticsManager _AnalyticsManager,
        ILocalizationManager _LocalizationManager,
        ILevelsLoader _LevelsLoader,
        IScoreManager _ScoreManager,
        IHapticsManager _HapticsManager)
    {
        ViewGameTicker = _ViewGameTicker;
        ModelGameTicker = _ModelGameTicker;
        UITicker = _UITicker;
        AdsManager = _AdsManager;
        AnalyticsManager = _AnalyticsManager;
        LocalizationManager = _LocalizationManager;
        LevelsLoader = _LevelsLoader;
        ScoreManager = _ScoreManager;
        HapticsManager = _HapticsManager;
    }


    #endregion
    
    #region engine methods
    
    private void Start()
    {
        Application.targetFrameRate = GraphicUtils.GetTargetFps();
        DataFieldsMigrator.InitDefaultDataFieldValues();
        InitGameManagers();
        LevelMonoInstaller.Release = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(SceneNames.Level);
    }
    
    private void OnSceneLoaded(Scene _Scene, LoadSceneMode _)
    {
        if (_Scene.name.EqualsIgnoreCase(SceneNames.Level))
        {
            GameClientUtils.GameId = 1; // если игра будет только одна, то и париться с GameId нет смысла
            InitGameController();
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #endregion
    
    #region nonpublic methods
    
    private void InitGameManagers()
    {
        GameClient.Instance.Init();
        ScoreManager       .Init();
        AdsManager         .Init();
        AnalyticsManager   .Init();
        LocalizationManager.Init();
        HapticsManager     .Init();
    }
    
    private void InitGameController()
    {
        Coroutines.Run(Coroutines.WaitWhile(
            () => !AssetBundleManager.BundlesLoaded,
            () =>
            {
                var controller = GameController.CreateInstance();
                controller.Initialized += () =>
                {
                    int levelIndex = SaveUtils.GetValue(SaveKeys.CurrentLevelIndex);
                    var info = LevelsLoader.LoadLevel(1, levelIndex);
                    controller.Model.LevelStaging.LoadLevel(info, levelIndex);
                };
                controller.Init();
            }));
    }


    #endregion
}