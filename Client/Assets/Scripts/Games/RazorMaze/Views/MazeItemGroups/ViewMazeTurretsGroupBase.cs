using System;
using System.Linq;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeItems;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public abstract class ViewMazeTurretsGroupBase : ViewMazeItemsGroupBase, IViewMazeTurretsGroup
    {
        #region inject

        protected IModelMazeData Data { get; }
        protected ICoordinateConverter Converter { get; }
        protected IContainersGetter ContainersGetter { get; }

        protected ViewMazeTurretsGroupBase(
            IModelMazeData _Data,
            IViewMazeCommon _Common, 
            ICoordinateConverter _Converter,
            IContainersGetter _ContainersGetter) : base(_Common)
        {
            Data = _Data;
            Converter = _Converter;
            ContainersGetter = _ContainersGetter;
        }

        #endregion
        
        #region api

        public override EMazeItemType[] Types => new[] {EMazeItemType.Turret};
        public event NoArgsHandler Initialized;

        public virtual void Init() => Initialized?.Invoke();

        public abstract void OnTurretShoot(TurretShotEventArgs _Args);

        #endregion

        public void OnBackgroundColorChanged(Color _Color)
        {
            foreach (var item in GetItems().Cast<IViewMazeItemTurret>().Where(_Item => _Item.Activated))
                item.OnBackgroundColorChanged(_Color);
        }
    }
}