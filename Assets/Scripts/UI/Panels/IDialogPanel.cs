using UI.Managers;
using UnityEngine;

namespace UI.Panels
{
    public interface IDialogPanel
    {
        UiCategory Category { get; }
        RectTransform Panel { get; }
        void Show();
    }
}