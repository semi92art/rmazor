using Constants;
using Extensions;
using Games.RazorMaze;
using Games.RazorMaze.Controllers;
using Managers;
using Ticker;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using Zenject;

public class IntroSceneViewer : MonoBehaviour
{
    private ITicker Ticker { get; set; }
    private ILevelsLoader LevelsLoader { get; set; }
    
    [Inject]
    public void Inject(ITicker _Ticker, ILevelsLoader _LevelsLoader)
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
        var controller = RazorMazeGameController.Instance;
        controller.PreInitialized += () =>
        {
            var levelScoreEntity = ScoreManager.Instance.GetMainScore();
            Coroutines.Run(Coroutines.WaitWhile(
                () => !levelScoreEntity.Loaded,
                () =>
                {
                    // int level = levelScoreEntity.Scores.First().Value;
                    // FIXME заглушка для загрузки какого-то уровня
                    var info = LevelsLoader.LoadLevel(GameClientUtils.GameId, 1, false);
                    controller.SetMazeInfo(info);
                    controller.Init();
                }));
        };
        controller.Initialized += () => controller.PostInit();
        controller.PreInit();
    }
}