using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItems
{
    public class ViewMazeItemTurretProt : IViewMazeItemTurret
    {
        public void PreShoot(TurretShotEventArgs _Args)
        {
            throw new System.NotImplementedException();
        }

        public void Shoot(TurretShotEventArgs _Args)
        {
            throw new System.NotImplementedException();
        }

        public object Clone()
        {
            throw new System.NotImplementedException();
        }

        public GameObject Object { get; }
        public bool Active { get; set; }
        public ViewMazeItemProps Props { get; set; }
        public void Init(ViewMazeItemProps _Props)
        {
            throw new System.NotImplementedException();
        }

        public void SetLocalPosition(Vector2 _Position)
        {
            throw new System.NotImplementedException();
        }

        public void SetLocalScale(float _Scale)
        {
            throw new System.NotImplementedException();
        }

        public bool Equal(MazeItem _MazeItem)
        {
            throw new System.NotImplementedException();
        }
    }
}