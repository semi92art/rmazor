using Games.RazorMaze.Views.MazeItems;

namespace Games.RazorMaze.Views.Common
{
    public interface IAppear
    {
        void Appear(bool _Appear);
        EAppearingState AppearingState { get; }
    }
}