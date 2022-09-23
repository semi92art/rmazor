using RMAZOR.Models.ItemProceeders;
using UnityEngine;

namespace RMAZOR.Views.MazeItems.Additional
{
    public interface IViewTurretProjectileTailFake : IViewTurretProjectileTail { }

    public class ViewTurretProjectileTailFake : IViewTurretProjectileTailFake
    {
        public object Clone()                                   => new ViewTurretProjectileTailFake();
        public void   Init(Transform               _Parent, GameObject _ProjectileObject) { }
        public void   ShowTail(TurretShotEventArgs _Args) { }
        public void   HideTail()                          { }
        public void   SetSortingOrder(int _Order)         { }
        public void   SetStencilRefId(int _RefId)         { }
    }
}