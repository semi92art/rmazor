using UICreationSystem.Factories;
using UnityEngine;
using Utils;

namespace UICreationSystem
{
    public class MainMenuUi
    {
        private RectTransform m_RectTransform;
        private RectTransform m_GameTitleContainer;
        private GameObject m_GameTitle;

        public MainMenuUi(RectTransform _Parent)
        {
            InitContainers(_Parent);

            SetGameTitle(SaveUtils.GetValue<int>(SaveKey.GameId));
        }

        private void InitContainers(RectTransform _Parent)
        {
            m_RectTransform = UiFactory.UiRectTransform(
                _Parent,
                "Main Menu",
                UiAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Vector2.one * 0.5f,
                Vector2.zero);
            
            m_GameTitleContainer = UiFactory.UiRectTransform(
                m_RectTransform,
                "Game Title",
                UiAnchor.Create(new Vector2(0.5f, 1.0f), new Vector2(0.5f, 1.0f)),
                new Vector2(0, -93f),
                Vector2.one * 0.5f,
                new Vector2(270f, 185f));
        }

        private void SetGameTitle(int _GameId)
        {
            switch (_GameId)
            {
                case 1:
                    m_GameTitle = PrefabInitializer.InitUiPrefab(
                        UiFactory.UiRectTransform(
                            m_GameTitleContainer,
                            "Pimple Killer Title",
                            UiAnchor.Create(Vector2.zero, Vector2.one), 
                            Vector2.zero, 
                            Vector2.one * 0.5f,
                            Vector2.zero),
                        "pimple_killer_main_menu", "game_title");
                    break;
                default:
                    SaveUtils.PutValue(SaveKey.GameId, 1); //TODO put id of default game (depends from build)
                    SetGameTitle(1);
                    break;
            }
        }
    }
}