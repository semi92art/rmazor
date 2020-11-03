using System;
using DebugConsole;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UICreationSystem
{
    [Flags]
    public enum UiCategory : short
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
        Settings = 256
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
        
        #endregion

        #region private members

        private UiCategory m_CurrentCategory = UiCategory.Nothing;

        #endregion

        #region engine methods
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.activeSceneChanged += (_Prev, _Next) =>
            {
                if (_Next.name == "Main")
                {
                    bool isLoadingFromLevel = _Prev.name == "Level";
                    MenuUi.Create(isLoadingFromLevel);
                }

#if !RELEASE
                DebugConsoleView.Create();
#endif
            };
        }
        
        #endregion
    }
}
