using Games.RazorMaze.Views.MazeItems;
using UnityEngine;

namespace UI.Panels
{
    public interface IDialogPanel
    {
        EUiCategory     Category       { get; }
        EAppearingState AppearingState { get; set; }
        RectTransform   PanelObject    { get; }
        void            LoadPanel();
        void            OnDialogShow();
        void            OnDialogHide();
        void            OnDialogEnable();
    }
}