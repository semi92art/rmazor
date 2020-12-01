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
        void CloseAll();
    }
}