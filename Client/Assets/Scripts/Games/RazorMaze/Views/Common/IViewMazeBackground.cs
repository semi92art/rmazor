using UnityEngine;

namespace Games.RazorMaze.Views.Common
{
    public interface IViewMazeBackground : IInit, IOnLevelStageChanged
    {
        Color BackgroundColor { get; }
        event ColorHandler BackgroundColorChanged;
    }
}