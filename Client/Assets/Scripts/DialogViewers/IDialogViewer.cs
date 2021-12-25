using Games.RazorMaze.Views.MazeItems;
using UI.Panels;
using UnityEngine;

namespace DialogViewers
{
    public interface IDialogViewerBase
    {
        IDialogPanel      CurrentPanel                { get; }
        RectTransform     Container                   { get; }
        System.Func<bool> IsOtherDialogViewersShowing { get; set; }
        void              Init(RectTransform _Parent);
    }
}