using Network;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader
{
    private const string WasNotMadeMessage = "Game was not made";
    
    public void LoadLevel()
    {
        switch (GameClient.Instance.GameId)
        {
            case 1:
                LoadPointsClicker();
                break;
            case 2:
                LoadFiguresDrawer();
                break;
            case 3:
                LoadMathTrain();
                break;
            case 4:
                LoadTilesPather();
                break;
            case 5:
                LoadBalanceDrawer();
                break;
            default:
                Debug.Log(WasNotMadeMessage);
                break;
        }
    }

    private void LoadPointsClicker()
    {
        SceneManager.LoadScene("Level");
    }

    private void LoadFiguresDrawer()
    {
        Debug.Log(WasNotMadeMessage);
    }

    private void LoadMathTrain()
    {
        SceneManager.LoadScene(WasNotMadeMessage);
    }

    private void LoadTilesPather()
    {
        SceneManager.LoadScene("Level");
    }

    private void LoadBalanceDrawer()
    {
        SceneManager.LoadScene(WasNotMadeMessage);
    }
}
