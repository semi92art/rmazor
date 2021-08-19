using System;
using Exceptions;
using UnityEngine;
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
        Shop = 32,
        Settings = 64,
        PlusMoney = 128
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
    
    public class UiManager : MonoBehaviour
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
            }
        }
        
        public GameUiCategory CurrentGameCategory
        {
            get => m_CurrentGameCategory;
            set
            {
                OnCurrentGameCategoryChanged?.Invoke(m_CurrentGameCategory, value);
                m_CurrentGameCategory = value;
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
            OnCurrentMenuCategoryChanged += OnMenuCategoryChanged;
            OnCurrentGameCategoryChanged += OnGameCategoryChanged;
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
                case MenuUiCategory.Shop:
                case MenuUiCategory.Settings:
                case MenuUiCategory.PlusMoney:
                    Application.targetFrameRate = GraphicUtils.GetMenuTargetFps();
                    break;
                default: 
                    throw new SwitchCaseNotImplementedException(_New);
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
                    throw new SwitchCaseNotImplementedException(_New);
            }
        }
        
        #endregion
    }
}
