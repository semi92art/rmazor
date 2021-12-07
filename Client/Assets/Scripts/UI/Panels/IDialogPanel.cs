using UnityEngine;

namespace UI.Panels
{
    public interface IDialogPanel
    {
        EUiCategory Category { get; }
        RectTransform Panel { get; }
        void LoadPanel();
        void OnDialogShow();
        void OnDialogHide();
        void OnDialogEnable();
    }
}