using UI.Managers;
using UnityEngine;

namespace UI.Panels
{
    public interface IDialogPanel
    {
        RectTransform Panel { get; }
        void Init();
        void OnDialogShow();
        void OnDialogHide();
        void OnDialogEnable();
    }
    
    public interface IMenuUiCategory
    {
        MenuUiCategory Category { get; }
    }

    public interface IGameUiCategory
    {
        GameUiCategory Category { get; }
    }
}