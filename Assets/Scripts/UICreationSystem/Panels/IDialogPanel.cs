using UnityEngine;

namespace UICreationSystem.Panels
{
    public interface IDialogPanel
    {
        UiCategory Category { get; }
        RectTransform Panel { get; }
        void Show();
    }
}