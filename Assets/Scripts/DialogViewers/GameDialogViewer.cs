using Helpers;
using UI;
using UI.Factories;
using UI.Managers;
using UI.Panels;
using UnityEngine;

namespace DialogViewers
{
    public class GameDialogViewer : MonoBehaviour, IGameDialogViewer
    {
        #region serialized fields
        
        public RectTransform dialogContainer;
        
        #endregion

        #region api

        public RectTransform DialogContainer => dialogContainer;

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
            throw new System.NotImplementedException();
        }
        
        public void Back()
        {
            throw new System.NotImplementedException();
        }

        public void AddNotDialogItem(RectTransform _Item, GameUiCategory _Categories)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveNotDialogItem(RectTransform _Item)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        
    }
}