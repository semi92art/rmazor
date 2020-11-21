using Helpers;
using UI;
using UI.Factories;
using UI.Managers;
using UI.Panels;
using UnityEngine;

namespace DialogViewers
{
    public class GameDialogViewer : DialogViewerBase, IGameDialogViewer
    {
        #region nonpublic members
        protected override float TransitionTime => 0.05f;
        
        #endregion
        
        #region api
        
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
    }
}