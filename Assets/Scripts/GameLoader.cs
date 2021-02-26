using DI;
using Managers;
using Network;
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
            ContainersManager.Instance.Clear();
            TimeOrLifesEndedPanel.TimesPanelCalled = 0;
        };
        
        GameClient.Instance.Init();
        AdsManager.Instance.Init();
        AnalyticsManager.Instance.Init();
        AssetBundleManager.Instance.Init();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        DebugConsole.DebugConsoleView.Instance.Init();
#endif
        SceneManager.LoadScene(1);
    }
    
    #endregion
}