using System;
using System.Linq;
using Constants;
using DI.Extensions;
using DialogViewers;
using GameHelpers;
using Ticker;
using UI.Entities;
using UI.Factories;
using UI.Managers;
using UI.PanelItems;
using UnityEngine;
using Utils;
using Object = UnityEngine.Object;

namespace UI.Panels
{
    public class SelectGamePanel : DialogPanelBase, IMenuUiCategory
    {
        #region private members
        
        private readonly IMenuDialogViewer m_DialogViewer;
        private readonly Action<int> m_SelectGame;

        #endregion

        #region api

        public MenuUiCategory Category => MenuUiCategory.SelectGame;

        public SelectGamePanel(
            IMenuDialogViewer _DialogViewer, 
            Action<int> _SelectGame,
            IUITicker _UITicker) : base(_UITicker)
        {
            m_DialogViewer = _DialogViewer;
            m_SelectGame = _SelectGame;
        }

        public override void Init()
        {
            GameObject selectGamePanel = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.Container,
                    RtrLites.FullFill),
                CommonPrefabSetNames.MainMenuDialogPanels,
                "select_game_panel");
            RectTransform content = selectGamePanel.GetCompItem<RectTransform>("content");
            
            GameObject cgiObj = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    content,
                    UiAnchor.Create(0, 1, 0, 1),
                    new Vector2(218f, -43f),
                    Vector2.one * 0.5f,
                    new Vector2(416f, 170f)),
                CommonPrefabSetNames.MainMenu,
                "select_game_item");
            
            foreach (var cgiProps in GetAllCgiProps().Where(_Props => _Props.IsVisible))
            {
                var cgiObjClone = cgiObj.Clone();
                ChooseGameItem cgi = cgiObjClone.GetComponent<ChooseGameItem>();
                cgiProps.Click = () =>
                {
                    GameClientUtils.GameId = cgiProps.GameId;
                    m_SelectGame.Invoke(cgiProps.GameId);
                    m_DialogViewer.Back();
                };
                cgi.Init(cgiProps, GetObservers(), UITicker);
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