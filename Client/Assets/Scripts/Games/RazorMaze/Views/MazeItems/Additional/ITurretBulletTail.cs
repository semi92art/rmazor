using Games.RazorMaze.Models.ItemProceeders;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface ITurretBulletTail
    {
        void Init();
        void ShowTail(TurretShotEventArgs _Args);
        void HideTail(TurretShotEventArgs _Args);
    }
}