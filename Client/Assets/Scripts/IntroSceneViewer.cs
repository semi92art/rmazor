﻿using System.Linq;
using Constants;
using DI.Extensions;
using GameHelpers;
using Games.RazorMaze.Controllers;
using Managers;
using Ticker;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Zenject;

public class IntroSceneViewer : MonoBehaviour
{
    private IGameTicker Ticker { get; set; }
    private ILevelsLoader LevelsLoader { get; set; }
    
    [Inject]
    public void Inject(IGameTicker _Ticker, ILevelsLoader _LevelsLoader)
    {
        Ticker = _Ticker;
        LevelsLoader = _LevelsLoader;
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
        SceneManager.LoadScene(SceneNames.Level);
    }

    private void InitGameController()
    {
        var controller = RazorMazeGameController.CreateInstance();
        controller.Initialized += () =>
        {
            var levelScoreEntity = ScoreManager.Instance.GetMainScore();
            Coroutines.Run(Coroutines.WaitWhile(
                () => !levelScoreEntity.Loaded,
                () =>
                {
                    int level = levelScoreEntity.Scores.First().Value;
                    Dbg.Log($"Current level from cache: {level}");
                    // FIXME заглушка для загрузки уровня
                    var info = LevelsLoader.LoadLevel(1, 1);
                    controller.Model.LevelStaging.LoadLevel(info, 1);
                    controller.Init();
                }));
        };
        controller.Init();
    }
}