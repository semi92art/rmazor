using DebugConsole;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UICreationSystem
{
    public enum UiCategory
    {
        Nothing,
        Loading,
        MainMenu,
        SelectGame,
        DailyBonus,
        WheelOfFortune,
        Profile,
        LoginOrRegistration,
        Shop,
        Settings
    }
    
    public class UiManager : MonoBehaviour, ISingleton
    {
        public static UiManager Instance { get; private set; }
        public delegate void UiStateHandler(UiCategory _Prev, UiCategory _New);
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
        
        private UiCategory m_CurrentCategory = UiCategory.Nothing;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.activeSceneChanged += (_Arg0, _Scene) =>
            {
                if (_Scene.name == "Main")
                    MenuUi.Create();
#if !RELEASE
                DebugConsoleView.Create();
#endif
            };
        }
    }
}
