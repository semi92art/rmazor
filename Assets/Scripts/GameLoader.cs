using Constants;
using Exceptions;
using UnityEngine.SceneManagement;
using Utils;

public static class GameLoader
{
    private static int _level;

    static GameLoader()
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
                
                break;
            default:
                throw new SwitchCaseNotImplementedException(_GameId);
        }
    }
}
