using UnityEngine;

namespace UI.Panels
{
    public interface IDialogPanel
    {
        EUiCategory Category { get; }
        RectTransform Panel { get; }
        void Init();
        void OnDialogShow();
        void OnDialogHide();
        void OnDialogEnable();
    }
}