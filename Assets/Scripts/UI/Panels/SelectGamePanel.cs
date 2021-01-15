using System.Linq;
using Constants;
using DialogViewers;
using Extensions;
using GameHelpers;
using Network;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UI.PanelItems;
using UnityEngine;

namespace UI.Panels
{
    public class SelectGamePanel : DialogPanelBase, IMenuUiCategory
    {
        #region private members
        
        private readonly IMenuDialogViewer m_DialogViewer;
        private readonly System.Action<int> m_SelectGame;

        #endregion

        #region api

        public MenuUiCategory Category => MenuUiCategory.SelectGame;

        public SelectGamePanel(
            IMenuDialogViewer _DialogViewer, 
            System.Action<int> _SelectGame)
        {
            m_DialogViewer = _DialogViewer;
            m_SelectGame = _SelectGame;
        }

        public override void Init()
        {
            GameObject selectGamePanel = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.Container,
                    RtrLites.FullFill),
                CommonStyleNames.MainMenuDialogPanels,
                "select_game_panel");
            RectTransform content = selectGamePanel.GetCompItem<RectTransform>("content");
            
            GameObject cgiObj = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    content,
                    UiAnchor.Create(0, 1, 0, 1),
                    new Vector2(218f, -43f),
                    Vector2.one * 0.5f,
                    new Vector2(416f, 170f)),
                CommonStyleNames.MainMenu,
                "select_game_item");
            
            foreach (var cgiProps in GetAllCgiProps().Where(_Props => _Props.IsVisible))
            {
                var cgiObjClone = cgiObj.Clone();
                ChooseGameItem cgi = cgiObjClone.GetComponent<ChooseGameItem>();
                cgiProps.Click = () =>
                {
                    GameClient.Instance.GameId = cgiProps.GameId;
                    m_SelectGame.Invoke(cgiProps.GameId);
                    m_DialogViewer.Back();
                };
                cgi.Init(cgiProps, GetObservers());
            }
            
            Object.Destroy(cgiObj);
            Panel = selectGamePanel.RTransform();
        }

        #endregion
        
        #region nonpublic methods

        private ChooseGameItemProps[] GetAllCgiProps()
        {
            var infos = GameInfo.Infos;
            return infos
                .Select(_Info => new ChooseGameItemProps(
                    _Info.GameId, _Info.Available))
                .ToArray();
        }
        
        #endregion
    }
}