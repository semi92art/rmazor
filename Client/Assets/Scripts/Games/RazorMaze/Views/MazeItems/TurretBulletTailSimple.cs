using Games.RazorMaze.Models.ItemProceeders;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface ITurretBulletTail
    {
        void Init();
        void ShowTail(TurretShotEventArgs _Args);
        void HideTail(TurretShotEventArgs _Args);
    }
    
    public class TurretBulletTailSimple : ITurretBulletTail
    {
        public void Init()
        {
            //throw new System.NotImplementedException();
        }

        public void ShowTail(TurretShotEventArgs _Args)
        {
            //throw new System.NotImplementedException();
        }

        public void HideTail(TurretShotEventArgs _Args)
        {
            //throw new System.NotImplementedException();
        }
    }
}