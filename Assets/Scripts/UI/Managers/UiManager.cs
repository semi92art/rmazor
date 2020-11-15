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
    public enum UiCategory : int
    {
        Nothing = 0,
        Loading = 1,
        MainMenu = 2,
        SelectGame = 4,
        DailyBonus = 8,
        WheelOfFortune = 16,
        Profile= 32,
        Login = 64,
        Shop = 128,
        Settings = 256,
        Countries = 512
    }
    
    public class UiManager : MonoBehaviour, ISingleton
    {
        #region singleton
        
        public static UiManager Instance { get; private set; }
        
        #endregion

        #region types

        public delegate void UiStateHandler(UiCategory _Prev, UiCategory _New);

        #endregion
        
        #region api
        
        public event UiStateHandler OnCurrentCategoryChanged;
        
        public UiCategory CurrentCategory
        {
            get => m_CurrentCategory;
            set
            {
                OnCurrentCategoryChanged?.Invoke(m_CurrentCategory, value);
                m_CurrentCategory = value;
            }
        }

        public string ColorScheme
        {
            get => SaveUtils.GetValue<string>(SaveKey.ColorScheme);
            set => SaveUtils.PutValue(SaveKey.ColorScheme, value);
        }
        
        #if DEBUG
        
        public GameObject DebugConsole { get; private set; }
        public GameObject DebugReporter { get; private set; }
        
        #endif
        
        #endregion

        #region private members

        private UiCategory m_CurrentCategory = UiCategory.Nothing;
        private string m_PrevScene;

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
                
#if DEBUG
                if (m_PrevScene == SceneNames.Preload)
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
                if (_Scene.name == SceneNames.Main)
                {
                    if (m_PrevScene == SceneNames.Preload)
                        LocalizationManager.Instance.Init();
                    MenuUi.Create(m_PrevScene == SceneNames.Preload);
                }

                m_PrevScene = _Scene.name;
            };
        }
        
        #endregion
    }
}
