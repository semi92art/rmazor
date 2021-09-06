using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.MazeItems;
using Ticker;

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
        
        public List<IViewMazeItem> MazeItems { get; private set; }
        
        public virtual void OnPathProceed(V2Int _PathItem)
        {
            var item = MazeItems.First(_Item => _Item.Props.Position == _PathItem && _Item.Props.IsNode);
            item.Proceeding = true;
        }

        public abstract void Init();
        public abstract IViewMazeItem GetItem(MazeItem _Item);
        public abstract T GetItem<T>(MazeItem _Item) where T : IViewMazeItem;
        public event NoArgsHandler GameLoopUpdate;

        public void UpdateTick()
        {
            GameLoopUpdate?.Invoke();
        }

        #endregion


    }
}