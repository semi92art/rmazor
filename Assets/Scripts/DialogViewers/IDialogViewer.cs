using UI.Panels;
using UnityEngine;

namespace DialogViewers
{
    public interface IDialogViewerBase
    {
        RectTransform Container { get; }
        void Back();
    }
    
    public interface IDialogViewer : IDialogViewerBase
    {
        void Show(IDialogPanel _ItemTo, bool _HidePrevious = true);
        void RemoveNotDialogItem(RectTransform _Item);
        void CloseAll();
    }
}