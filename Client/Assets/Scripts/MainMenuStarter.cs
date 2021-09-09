using System;
using Constants;
using Controllers;
using Entities;
using Extensions;
using Games.RazorMaze;
using Managers;
using Ticker;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Zenject;

public class MainMenuStarter : MonoBehaviour
{
    [Inject]
    
    private ITicker Ticker { get; set; }
    
    public void Inject(ITicker _Ticker)
    {
        Ticker = _Ticker;
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        CommonUtils.LoadSceneCorrectly(Ticker, SceneNames.Level);
    }

    private void OnSceneLoaded(Scene _Scene, LoadSceneMode _Mode)
    {
        if (_Scene.name.EqualsIgnoreCase(SceneNames.Level))
        {
            var levelScoreEntity = ScoreManager.Instance.GetMainScore();
            Coroutines.Run(Coroutines.WaitWhile(
                () => !levelScoreEntity.Loaded,
                () =>
                {
                    // int level = levelScoreEntity.Scores.First().Value;
                    // FIXME заглушка для загрузки какого-то уровня
                    var info = MazeLevelUtils.LoadLevel(
                        GameClientUtils.GameId, 1, 1, MazeLevelUtils.HeapIndexRelease, false);
                    RazorMazeGameController.Instance.PreInit();
                    RazorMazeGameController.Instance.SetMazeInfo(info);
                    RazorMazeGameController.Instance.Init();
                    RazorMazeGameController.Instance.PostInit();
                }));
        }
    }
}