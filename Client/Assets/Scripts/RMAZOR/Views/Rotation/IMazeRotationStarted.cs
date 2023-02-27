using RMAZOR.Models;

namespace RMAZOR.Views.Rotation
{
    public interface IMazeRotationStarted
    {
        void OnMazeRotationStarted(MazeRotationEventArgs _Args);
    }
}