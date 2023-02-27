using RMAZOR.Models;

namespace RMAZOR.Views.Rotation
{
    public interface IMazeRotationFinished
    {
        void OnMazeRotationFinished(MazeRotationEventArgs _Args);
    }
}