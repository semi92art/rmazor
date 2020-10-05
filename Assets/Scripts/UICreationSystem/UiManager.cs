using UnityEngine;
using UnityEngine.SceneManagement;

namespace UICreationSystem
{
    public class UiManager : MonoBehaviour
    {
        #region static properties
        public static UiManager Instance { get; private set; }

        #endregion

        #region private fields

        private MenuUI m_Menu;

        #endregion

        #region engine methods

        private void Awake()
        {
            if (Instance != null)
            {
                DestroyImmediate(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.activeSceneChanged += (_Arg0, _Scene) =>
            {
                if (_Scene.name == "Main")
                    m_Menu = new MenuUI();
            };
        }

        #endregion
    }
}
