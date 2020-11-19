using System.Collections.Generic;
using System.Linq;
using DialogViewers;
using Extensions;
using Helpers;
using Network;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UI.PanelItems;
using UnityEngine;

namespace UI.Panels
{
    public class SelectGamePanel : IDialogPanel
    {
        #region private members
        
        private readonly IDialogViewer m_DialogViewer;

        private readonly List<ChooseGameItemProps> m_CgiPropsList = new List<ChooseGameItemProps>
        {
            new ChooseGameItemProps(1, false, true),
            new ChooseGameItemProps(2, false, true),
            new ChooseGameItemProps(3, false, false),
            new ChooseGameItemProps(4, false, true),
            new ChooseGameItemProps(5, false, false)
        };
        
        private System.Action<int> m_SelectGame;
        
        #endregion

        #region api

        public UiCategory Category => UiCategory.SelectGame;
        public RectTransform Panel { get; private set; }
        
        public SelectGamePanel(IDialogViewer _DialogViewer, System.Action<int> _SelectGame)
        {
            m_DialogViewer = _DialogViewer;
            m_SelectGame = _SelectGame;
        }

        public void Show()
        {
            Panel = Create();
            m_DialogViewer.Show(this);
        }

        #endregion
        
        #region private methods
        
        private RectTransform Create()
        {
            GameObject selectGamePanel = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "main_menu",
                "select_game_panel");
            RectTransform content = selectGamePanel.GetCompItem<RectTransform>("content");
            
            GameObject cgiObj = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    content,
                    UiAnchor.Create(0, 1, 0, 1),
                    new Vector2(218f, -43f),
                    Vector2.one * 0.5f,
                    new Vector2(416f, 170f)),
                "main_menu",
                "select_game_item");
            
            foreach (var cgiProps in m_CgiPropsList.Where(_Props => _Props.IsVisible))
            {
                var cgiObjClone = cgiObj.Clone();
                ChooseGameItem cgi = cgiObjClone.GetComponent<ChooseGameItem>();
                cgiProps.Click = () =>
                {
                    GameClient.Instance.GameId = cgiProps.GameId;
                    m_SelectGame.Invoke(cgiProps.GameId);
                    m_DialogViewer.Back();
                };
                cgi.Init(cgiProps);
            }
            
            Object.Destroy(cgiObj);
            return selectGamePanel.RTransform();
        }
        
        
        
        #endregion
    }
}