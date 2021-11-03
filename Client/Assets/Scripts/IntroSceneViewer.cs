using Constants;
using DI.Extensions;
using Entities;
using GameHelpers;
using Games.RazorMaze.Controllers;
using Managers;
using Mono_Installers;
using Ticker;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Zenject;

public class IntroSceneViewer : MonoBehaviour
{
    private IGameTicker Ticker { get; set; }
    private ILevelsLoader LevelsLoader { get; set; }
    private IScoreManager ScoreManager { get; set; }

    [Inject]
    public void Inject(
        IGameTicker _Ticker, 
        ILevelsLoader _LevelsLoader, 
        IScoreManager _ScoreManager)
    {
        Ticker = _Ticker;
        LevelsLoader = _LevelsLoader;
        ScoreManager = _ScoreManager;
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        ShowIntroScene();
    }

    private void OnSceneLoaded(Scene _Scene, LoadSceneMode _Mode)
    {
        if (_Scene.name.EqualsIgnoreCase(SceneNames.Level))
            InitGameController();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void ShowIntroScene()
    {
        // TODO здесь показывать интруху
        
        Ticker.ClearRegisteredObjects();
        LevelMonoInstaller.Release = true;
        SceneManager.LoadScene(SceneNames.Level);
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
                    int levelIndex = SaveUtils.GetValue<int>(SaveKey.CurrentLevelIndex);
                    var info = LevelsLoader.LoadLevel(1, levelIndex);
                    controller.Model.LevelStaging.LoadLevel(info, levelIndex);
                };
                controller.Init();
            }));
    }
}