using UI.Managers;
using UnityEngine;

namespace UI.Panels
{
    public interface IDialogPanel
    {
        RectTransform Panel { get; }
        void Show();
    }
    
    public interface IMenuDialogPanel : IDialogPanel
    {
        MenuUiCategory Category { get; }
    }

    public interface IGameDialogPanel : IDialogPanel
    {
        GameUiCategory Category { get; }
    }
}