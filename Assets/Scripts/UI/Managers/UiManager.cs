using System;
using Constants;
using Controllers;
using DebugConsole;
using Entities;
using Exceptions;
using Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace UI.Managers
{
    [Flags]
    public enum MenuUiCategory
    {
        Nothing = 0,
        Loading = 1,
        MainMenu = 2,
        SelectGame = 4,
        DailyBonus = 8,
        WheelOfFortune = 16,
        Profile = 32,
        Login = 64,
        Shop = 128,
        Settings = 256,
        PlusMoney = 512,
        PlusLifes = 1024
    }

    [Flags]
    public enum GameUiCategory
    {
        Nothing = 0,
        LevelStart = 1,
        Countdown = 2,
        Game = 4,
        TimeEnded = 8,
        LevelFinish = 16,
        Settings = 32
    }
    
    public class UiManager : MonoBehaviour, ISingleton
    {
        #region singleton
        
        public static UiManager Instance { get; private set; }
        
        #endregion

        #region types

        public delegate void MenuUiStateHandler(MenuUiCategory _Prev, MenuUiCategory _New);
        public delegate void GameUiStateHandler(GameUiCategory _Prev, GameUiCategory _New);

        #endregion
        
        #region nonpublic members

        private MenuUiCategory m_CurrentMenuCategory = MenuUiCategory.Nothing;
        private GameUiCategory m_CurrentGameCategory = GameUiCategory.Nothing;
        private string m_PrevScene;

        #endregion
        
        #region api
        
        public event MenuUiStateHandler OnCurrentMenuCategoryChanged;
        public event GameUiStateHandler OnCurrentGameCategoryChanged;
        
        public MenuUiCategory CurrentMenuCategory
        {
            get => m_CurrentMenuCategory;
            set
            {
                OnCurrentMenuCategoryChanged?.Invoke(m_CurrentMenuCategory, value);
                m_CurrentMenuCategory = value;
                if (value != MenuUiCategory.Nothing)
                    OnCurrentGameCategoryChanged = null;
            }
        }
        
        public GameUiCategory CurrentGameCategory
        {
            get => m_CurrentGameCategory;
            set
            {
                OnCurrentGameCategoryChanged?.Invoke(m_CurrentGameCategory, value);
                m_CurrentGameCategory = value;
                if (value != GameUiCategory.Nothing)
                    OnCurrentMenuCategoryChanged = null;
            }
        }

#if !UNITY_EDITOR && DEVELOPMENT_BUILD
        public GameObject DebugReporter { get; private set; }
#endif
        
        #endregion

        #region engine methods
        
        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            OnCurrentMenuCategoryChanged += OnMenuCategoryChanged;
            OnCurrentGameCategoryChanged += OnGameCategoryChanged;
        }

        private void OnSceneLoaded(Scene _Scene, LoadSceneMode _)
        {
            bool onStart = m_PrevScene.EqualsIgnoreCase(SceneNames.Preload);
                
            if (onStart)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                bool debugOn = SaveUtils.GetValue<bool>(SaveKeyDebug.DebugUtilsOn);
                SaveUtils.PutValue(SaveKeyDebug.DebugUtilsOn, debugOn);
                DebugConsoleView.Instance.SetGoActive(debugOn);
    #if !UNITY_EDITOR && DEVELOPMENT_BUILD
                    DebugReporter = GameHelpers.PrefabInitializer.InitPrefab(
                        null,
                        "debug_console",
                        "reporter");
                    DebugReporter.SetActive(debugOn);
    #endif
#endif
            }
                
            if (_Scene.name.EqualsIgnoreCase(SceneNames.Main))
            {
                if (onStart)
                    LocalizationManager.Instance.Init();
                var menuUi = new MenuUi(onStart);
                menuUi.AddObserver(new MenuUiSoundController());
                menuUi.Init();
            }

            m_PrevScene = _Scene.name;
        }
        
        private void OnMenuCategoryChanged(MenuUiCategory _, MenuUiCategory _New)
        {
            switch (_New)
            {
                case MenuUiCategory.Nothing:
                    // do nothing
                    break;
                case MenuUiCategory.WheelOfFortune:
                    Application.targetFrameRate = GraphicUtils.GetGameTargetFps();
                    break;
                case MenuUiCategory.Loading:
                case MenuUiCategory.MainMenu:
                case MenuUiCategory.SelectGame:
                case MenuUiCategory.DailyBonus:
                case MenuUiCategory.Profile:
                case MenuUiCategory.Login:
                case MenuUiCategory.Shop:
                case MenuUiCategory.Settings:
                case MenuUiCategory.PlusMoney:
                case MenuUiCategory.PlusLifes:
                    Application.targetFrameRate = GraphicUtils.GetMenuTargetFps();
                    break;
                default: 
                    throw new InvalidEnumArgumentExceptionEx(_New);
            }
        }
        
        private void OnGameCategoryChanged(GameUiCategory _, GameUiCategory _New)
        {
            switch (_New)
            {
                case GameUiCategory.Nothing:
                    //do nothing
                    break;
                case GameUiCategory.Game:
                    Application.targetFrameRate = GraphicUtils.GetGameTargetFps();
                    break;
                case GameUiCategory.LevelStart:
                case GameUiCategory.Countdown:
                case GameUiCategory.TimeEnded:
                case GameUiCategory.LevelFinish:
                case GameUiCategory.Settings:
                    Application.targetFrameRate = GraphicUtils.GetMenuTargetFps();
                    break;
                default:
                    throw new InvalidEnumArgumentExceptionEx(_New);
            }
        }
        
        #endregion
    }
}
