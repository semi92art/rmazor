using Constants;
using Exceptions;
using Games.PointsTapper;
using Network;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

public static class LevelLoader
{
    private const string WasNotMadeMessage = "Game was not made";
    private static int _level;

    static LevelLoader()
    {
        SceneManager.sceneLoaded += OnLoadLevel;
    }

    private static void OnLoadLevel(Scene _Scene, LoadSceneMode _LoadSceneMode)
    {
        if (_Scene.name != SceneNames.Level)
            return;
        LoadGame(GameClientUtils.GameId);
    }

    public static void LoadLevel(int _Level)
    {
        _level = _Level;
        SceneManager.LoadScene(SceneNames.Level);
    }
    
    private static void LoadGame(int _GameId)
    {
        switch (_GameId)
        {
            case 1:
                PointsTapperManager.Instance.Init(_level);
                break;
            case 2:
                //LinesDefenderManager.Instance.Init(_level);
                break;
            case 3:
                Dbg.Log(WasNotMadeMessage);
                break;
            case 4:
                Dbg.Log(WasNotMadeMessage);
                break;
            case 5:
                Dbg.Log(WasNotMadeMessage);
                break;
            default:
                throw new SwitchCaseNotImplementedException(_GameId);
        }
    }
}
