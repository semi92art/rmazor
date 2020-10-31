using Network;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{
    private void Start()
    {
        GoogleAdsManager.Instance.Init();
        GameClient.Instance.Init();
        SceneManager.LoadScene(1);
    }
}