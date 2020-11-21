using UI.Managers;
using UI.Panels;
using UnityEngine;

namespace DialogViewers
{
    public interface IDialogViewer
    {
        RectTransform DialogContainer { get; }
        void Back();
        void RemoveNotDialogItem(RectTransform _Item);
    }
    
    public interface IMenuDialogViewer : IDialogViewer
    {
        void Show(IMenuDialogPanel _ItemTo, bool _HidePrevious = true);
        void AddNotDialogItem(RectTransform _Item, MenuUiCategory _Categories);
    }

    public interface IGameDialogViewer : IDialogViewer
    {
        void Show(IGameDialogPanel _ItemTo, bool _HidePrevious = true);
        void AddNotDialogItem(RectTransform _Item, GameUiCategory _Categories);
    }
}