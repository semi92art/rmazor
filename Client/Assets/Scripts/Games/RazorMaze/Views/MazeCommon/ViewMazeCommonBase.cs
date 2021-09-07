using System.Collections.Generic;
using System.Linq;
using Entities;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.Helpers.MazeItemsCreators;
using Games.RazorMaze.Views.MazeItems;
using Ticker;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeCommon
{
    public abstract class ViewMazeCommonBase : IViewMazeCommon, IUpdateTick
    {
        #region inject

        protected ITicker Ticker { get; }
        protected IMazeItemsCreator MazeItemsCreator { get; }
        protected IModelMazeData Model { get; }
        protected IContainersGetter ContainersGetter { get; }
        protected ICoordinateConverter CoordinateConverter { get; }

        protected ViewMazeCommonBase(
            ITicker _Ticker,
            IMazeItemsCreator _MazeItemsCreator,
            IModelMazeData _Model,
            IContainersGetter _ContainersGetter, 
            ICoordinateConverter _CoordinateConverter)
        {
            Ticker = _Ticker;
            MazeItemsCreator = _MazeItemsCreator;
            Model = _Model;
            ContainersGetter = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
            
            Ticker.Register(this);
        }

        #endregion
        
        #region api
        
        public event NoArgsHandler GameLoopUpdate;
        public abstract List<IViewMazeItem> MazeItems { get; }

        public virtual void Init()
        {
            ContainersGetter.MazeItemsContainer.SetLocalPosXY(Vector2.zero);
            ContainersGetter.MazeItemsContainer.PlusLocalPosY(CoordinateConverter.GetScale() * 0.5f);
        }
        
        public virtual void OnPathProceed(V2Int _PathItem)
        {
            var item = MazeItems.First(_Item => _Item.Props.Position == _PathItem && _Item.Props.IsNode);
            item.Proceeding = true;
        }
        
        public virtual void UpdateTick() => GameLoopUpdate?.Invoke();

        public abstract IViewMazeItem GetItem(MazeItem _Item);
        public virtual T GetItem<T>(MazeItem _Item)  where T : IViewMazeItem => (T) GetItem(_Item);

        #endregion


    }
}