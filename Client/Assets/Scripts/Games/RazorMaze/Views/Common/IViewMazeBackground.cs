using UnityEngine;
using UnityEngine.Events;

namespace Games.RazorMaze.Views.Common
{
    public interface IViewMazeBackground : IInit, IOnLevelStageChanged
    {
        Color BackgroundColor { get; }
        event UnityAction<Color> BackgroundColorChanged;
    }
}