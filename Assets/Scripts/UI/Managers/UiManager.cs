using System;
using Constants;
using DebugConsole;
using Entities;
using Helpers;
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
        Countries = 512
    }

    [Flags]
    public enum GameUiCategory
    {
        Nothing = 0,
        GameStart = 1,
        Game = 2,
        Settings = 4,
        LevelResults = 8
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
        
        #region private members

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

        public string ColorScheme
        {
            get => SaveUtils.GetValue<string>(SaveKey.ColorScheme);
            set => SaveUtils.PutValue(SaveKey.ColorScheme, value);
        }
        
#if DEBUG
        
        public GameObject DebugConsole { get; private set; }
#if !UNITY_EDITOR
        public GameObject DebugReporter { get; private set; }
#endif
        
#endif
        
        #endregion
        
        #region engine methods
        
        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (string.IsNullOrEmpty(ColorScheme))
                ColorScheme = "Default";

            SceneManager.sceneLoaded += (_Scene, _) =>
            {
                bool onStart = String.Compare(m_PrevScene, 
                                              SceneNames.Preload, StringComparison.OrdinalIgnoreCase) == 0;
#if DEBUG
                if (onStart)
                {
                    bool debugOn = SaveUtils.GetValue<bool>(SaveKey.SettingDebug);
                    SaveUtils.PutValue(SaveKey.SettingDebug, debugOn);
                    DebugConsole = DebugConsoleView.Create();
                    DebugConsole.SetActive(debugOn);
#if !UNITY_EDITOR
                    DebugReporter = PrefabInitializer.InitPrefab(
                        null,
                        "debug_console",
                        "reporter");
                    DebugReporter.SetActive(debugOn);
#endif
                }
#endif
                if (String.Compare(_Scene.name, SceneNames.Main, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (onStart)
                        LocalizationManager.Instance.Init();
                    MenuUi.Create(onStart);
                }

                m_PrevScene = _Scene.name;
            };
        }
        
        #endregion
    }
}
