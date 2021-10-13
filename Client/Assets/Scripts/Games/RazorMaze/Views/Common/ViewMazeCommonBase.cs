using System.Collections.Generic;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ProceedInfos;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers.MazeItemsCreators;
using Games.RazorMaze.Views.MazeItems;
using Ticker;
using UnityEngine.Events;

namespace Games.RazorMaze.Views.Common
{
    public abstract class ViewMazeCommonBase : IViewMazeCommon
    {
        #region inject

        protected IGameTicker GameTicker { get; }
        protected IMazeItemsCreator MazeItemsCreator { get; }
        protected IModelData ModelData { get; }
        protected IContainersGetter ContainersGetter { get; }
        protected ICoordinateConverter CoordinateConverter { get; }

        protected ViewMazeCommonBase(
            IGameTicker _GameTicker,
            IMazeItemsCreator _MazeItemsCreator,
            IModelData _ModelData,
            IContainersGetter _ContainersGetter, 
            ICoordinateConverter _CoordinateConverter)
        {
            GameTicker = _GameTicker;
            MazeItemsCreator = _MazeItemsCreator;
            ModelData = _ModelData;
            ContainersGetter = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;

            GameTicker.Register(this);
        }

        #endregion
        
        #region api
        
        public event UnityAction Initialized;
        public abstract List<IViewMazeItem> MazeItems { get; }
        public virtual void Init() => Initialized?.Invoke();
        public abstract IViewMazeItem GetItem(IMazeItemProceedInfo _Info);
        public virtual T GetItem<T>(IMazeItemProceedInfo _Info)  where T : IViewMazeItem => (T) GetItem(_Info);
        public abstract void OnLevelStageChanged(LevelStageArgs _Args);

        #endregion
    }
}