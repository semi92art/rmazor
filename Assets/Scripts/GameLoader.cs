using Managers;
using Network;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour, ISingleton
{
    #region engine methods

    private void Start()
    {
        GoogleAdsManager.Instance.Init();
        GameClient.Instance.Init();
        SceneManager.LoadScene(1);
    }
    
    #endregion
}