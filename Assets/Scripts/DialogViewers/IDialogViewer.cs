using UI.Managers;
using UI.Panels;
using UnityEngine;

namespace DialogViewers
{
    public interface IDialogViewer
    {
        RectTransform DialogContainer { get; }
        void Show(IDialogPanel _ItemTo, bool _HidePrevious = true);
        void Back();
        void AddNotDialogItem(RectTransform _Item, UiCategory _Categories);
        void RemoveNotDialogItem(RectTransform _Item);
    }
}