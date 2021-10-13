using System.Linq;
using Constants;
using DI.Extensions;
using DialogViewers;
using Entities;
using GameHelpers;
using Ticker;
using UI.Entities;
using UI.Factories;
using UI.PanelItems;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace UI.Panels
{
    public interface ISelectGameDialogPanel : IDialogPanel
    {
        UnityAction<int> OnSelectGame { get; set; }
    }
    
    public class SelectGamePanel : DialogPanelBase, ISelectGameDialogPanel
    {
        #region inject

        public SelectGamePanel(
            IDialogViewer _DialogViewer,
            IManagersGetter _Managers,
            IUITicker _UITicker) 
            : base(_Managers, _UITicker, _DialogViewer) { }

        #endregion
        
        #region api

        public UnityAction<int> OnSelectGame { get; set; }
        public override EUiCategory Category => EUiCategory.SelectGame;

        public override void Init()
        {
            base.Init();
            GameObject selectGamePanel = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    DialogViewer.Container,
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
                    OnSelectGame.Invoke(cgiProps.GameId);
                    DialogViewer.Back();
                };
                cgi.Init(cgiProps, Managers);
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