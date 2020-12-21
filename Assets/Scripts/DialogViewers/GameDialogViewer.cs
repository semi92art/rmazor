using System.Linq;
using GameHelpers;
using UI;
using UI.Factories;
using UI.Managers;
using UI.Panels;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace DialogViewers
{
    public interface IGameDialogViewer : IDialogViewer
    {
        void Show(IGameDialogPanel _ItemTo, bool _HidePrevious = true);
        void AddNotDialogItem(RectTransform _Item, GameUiCategory _Categories);
        IGameDialogPanel Last { get; }
    }
    
    public class GameDialogViewer : DialogViewerBase, IGameDialogViewer
    {
        #region api

        public IGameDialogPanel Last => PanelStack.Any() ? PanelStack.Peek().GameDialogPanel : null;
        
        public static IGameDialogViewer Create(RectTransform _Parent)
        {
            var dialogPanelObj = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    _Parent,
                    RtrLites.FullFill),
                "dialog_viewers",
                "game_viewer");
            return dialogPanelObj.GetComponent<GameDialogViewer>();
        }

        public void Show(IGameDialogPanel _ItemTo, bool _HidePrevious = true)
        {
            var to = new Panel(_ItemTo);
            ShowCore(to, _HidePrevious, false, GameUiCategoryType);
        }
 
        public void AddNotDialogItem(RectTransform _Item, GameUiCategory _Categories)
        {
            if (NotDialogs.ContainsKey(_Item))
                return;
            NotDialogs.Add(_Item, new VisibleInCategories(_Categories, _Item.gameObject.activeSelf));
            GraphicsAlphas.Add(_Item.GetInstanceID(), new GraphicAlphas(_Item));
        }

        #endregion
        
        #region engine methods

        protected override void Update()
        {
            //Do nothing
        }
        
        #endregion
    }
}