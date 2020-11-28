using Constants;
using DI;
using Managers;
using Network;
using PointsTapper;
using UI.Panels;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour, ISingleton
{
    #region engine methods

    private void Start()
    {
        SceneManager.sceneLoaded += (_Scene, _Mode) =>
        {
            ContainersManager.Instance = null;
            var containersManager = FindObjectOfType<ContainersManager>();
            if (containersManager == null) 
                return;
            Destroy(containersManager.gameObject);

            TimeOrLifesEndedPanel.TimesPanelCalled = 0;
        };
        
        GoogleAdsManager.Instance.Init();
        GameClient.Instance.Init();
        SceneManager.LoadScene(1);
    }
    
    #endregion
}