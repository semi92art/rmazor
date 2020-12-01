﻿using DI;
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
        
        GoogleAdsManager.Instance.Init();
        GameClient.Instance.Init();
        SceneManager.LoadScene(1);
    }
    
    #endregion
}