using Games.RazorMaze.Models;
using UnityEngine;
using Games.RazorMaze.Models.ItemProceeders;

namespace Games.RazorMaze.Views.MazeItems
{
    public abstract class ViewMazeItemProtFakeBase : IViewMazeItem
    {
        public GameObject Object { get; }
        public bool Active { get; set; }
        public ViewMazeItemProps Props { get; set; }
        
        public void Init(ViewMazeItemProps _Props) => throw new System.NotImplementedException();
        public void SetLocalPosition(Vector2 _Position) => throw new System.NotImplementedException();
        public void SetLocalScale(float _Scale) => throw new System.NotImplementedException();
        public bool Equal(MazeItem _MazeItem) => throw new System.NotImplementedException();
        public object Clone() => throw new System.NotImplementedException();
    }
    public class ViewMazeItemSpringboardProtFake : ViewMazeItemProtFakeBase, IViewMazeItemSpringboard
    {
        public void MakeJump(SpringboardEventArgs _Args) => throw new System.NotImplementedException();
    }
    public class ViewMazeItemShredingerBlockProtFake : ViewMazeItemProtFakeBase, IViewMazeItemShredingerBlock { }
    public class ViewMazeItemTurretProtFake : ViewMazeItemProtFakeBase, IViewMazeItemTurret
    {
        public void PreShoot(TurretShotEventArgs _Args) => throw new System.NotImplementedException();
        public void Shoot(TurretShotEventArgs _Args) => throw new System.NotImplementedException();
    }
    public class ViewMazeItemGravityBlockProtFake : ViewMazeItemProtFakeBase, IViewMazeItemGravityBlock
    {
        public void OnMoveStarted(MazeItemMoveEventArgs _Args) => throw new System.NotImplementedException();
        public void OnMoving(MazeItemMoveEventArgs _Args) => throw new System.NotImplementedException();
        public void OnMoveFinished(MazeItemMoveEventArgs _Args) => throw new System.NotImplementedException();
    }
    public class ViewMazeItemMovingTrapProtFake : ViewMazeItemProtFakeBase, IViewMazeItemMovingTrap
    {
        public void OnMoveStarted(MazeItemMoveEventArgs _Args) => throw new System.NotImplementedException();
        public void OnMoving(MazeItemMoveEventArgs _Args) => throw new System.NotImplementedException();
        public void OnMoveFinished(MazeItemMoveEventArgs _Args) => throw new System.NotImplementedException();
    }
    public class ViewMazeItemPathProtFake : ViewMazeItemProtFakeBase, IViewMazeItemPath { }
    public class ViewMazeItemPortalProtFake : ViewMazeItemProtFakeBase, IViewMazeItemPortal
    {
        public void DoTeleport(PortalEventArgs _Args) => throw new System.NotImplementedException();
    }
    public class ViewMazeItemGravityTrapProtFake : ViewMazeItemProtFakeBase, IViewMazeItemGravityTrap
    {
        public void OnMoveStarted(MazeItemMoveEventArgs _Args) => throw new System.NotImplementedException();
        public void OnMoving(MazeItemMoveEventArgs _Args) => throw new System.NotImplementedException();
        public void OnMoveFinished(MazeItemMoveEventArgs _Args) => throw new System.NotImplementedException();
    }
}