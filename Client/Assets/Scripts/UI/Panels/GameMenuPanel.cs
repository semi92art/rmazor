using DI.Extensions;
using DialogViewers;
using Entities;
using GameHelpers;
using Ticker;
using UI.Factories;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Panels
{


    public interface IGameMenuDialogPanel : IDialogPanel
    {
        UnityAction OnContinue { get; set; }
    }
    
    public class GameMenuPanel : DialogPanelBase, IGameMenuDialogPanel
    {
        #region inject
        

        public GameMenuPanel(
            IDialogViewer _DialogViewer,
            IManagersGetter _Managers,
            IUITicker _UITicker) 
            : base(_Managers, _UITicker, _DialogViewer) { }

        #endregion
        
        #region api
        
        public UnityAction OnContinue { get; set; }
        public override EUiCategory Category => EUiCategory.Settings;

        public override void OnDialogShow()
        {
            DialogViewer.Show(this);
        }

        public override void Init()
        {
            base.Init();
            GameObject go = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    DialogViewer.Container,
                    RtrLites.FullFill),
                "game_menu", "game_menu_panel");

            Panel = go.RTransform();
        }


        #endregion
    }
}