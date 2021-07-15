using System;
using Controllers;
using Entities;
using UI;
using UnityEngine;
using Zenject;

public class MainMenuStarter : MonoBehaviour
{
    private IMainMenuUiLoader MainMenuUiLoader { get; set; }
    
    [Inject]
    public void Inject(IMainMenuUiLoader _MainMenuUiLoader)
    {
        MainMenuUiLoader = _MainMenuUiLoader;
    }

    private void Start()
    {
        (MainMenuUiLoader as GameObservable)?.AddObserver(new MenuUiSoundController());
        MainMenuUiLoader.Init(true);
    }
}