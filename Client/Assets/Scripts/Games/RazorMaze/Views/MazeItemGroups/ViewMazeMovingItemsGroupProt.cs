using System.Collections.Generic;
using System.Linq;
using Entities;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.MazeCommon;
using Shapes;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeMovingItemsGroupProt : ViewMazeMovingItemsGroupBase
    {
        #region inject
        
        public ViewMazeMovingItemsGroupProt(
            IModelMazeData _Data,
            IMovingItemsProceeder _MovingItemsProceeder,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IViewMazeCommon _MazeCommon) 
            : base(_Data, _MovingItemsProceeder, _CoordinateConverter, _ContainersGetter, _MazeCommon) { }
        
        #endregion
        
        #region api

        public override void Init()
        {
            DrawWallBlockMovingPaths(Color.black);
            base.Init();
        }

        #endregion

        #region nonpublic methods
        
        protected override void MarkMazeItemBusyPositions(MazeItem _Item, IEnumerable<V2Int> _Positions)
        {
            if (!m_ItemsMoving.ContainsKey(_Item))
                m_ItemsMoving.Add(_Item, new ViewMovingItemInfo
                {
                    BusyPositions = new Dictionary<V2Int, Disc>()
                });

            var busyPoss = m_ItemsMoving[_Item].BusyPositions;

            foreach (var kvp in busyPoss.Where(_Kvp => !_Kvp.Value.IsNull()))
                kvp.Value.DestroySafe();
            busyPoss.Clear();

            if (_Positions == null)
                return;
            
            foreach (var pos in _Positions)
            {
                var go = new GameObject("Busy Pos");
                go.SetParent(ContainersGetter.MazeContainer);
                go.transform.localPosition = CoordinateConverter.ToLocalCharacterPosition(pos);
                var disc = go.AddComponent<Disc>();
                disc.Color = Color.red;
                disc.Radius = 0.3f;
                disc.SortingOrder = 20;
                busyPoss.Add(pos, disc);
            }
        }
        
        #endregion
    }
}