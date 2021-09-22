using System.Collections.Generic;
using DI.Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers.MazeItemsCreators;
using Games.RazorMaze.Views.MazeItems;
using Ticker;
using TimeProviders;
using UnityEngine;

namespace Games.RazorMaze.Views.Common
{
    public abstract class ViewMazeCommonBase : IViewMazeCommon, IUpdateTick
    {
        #region inject

        protected IGameTicker GameTicker { get; }
        protected IMazeItemsCreator MazeItemsCreator { get; }
        protected IModelMazeData ModelData { get; }
        protected IContainersGetter ContainersGetter { get; }
        protected ICoordinateConverter CoordinateConverter { get; }
        protected IGameTimeProvider GameTimeProvider { get; }

        protected ViewMazeCommonBase(
            IGameTicker _GameTicker,
            IMazeItemsCreator _MazeItemsCreator,
            IModelMazeData _ModelData,
            IContainersGetter _ContainersGetter, 
            ICoordinateConverter _CoordinateConverter,
            IGameTimeProvider _GameTimeProvider)
        {
            GameTicker = _GameTicker;
            MazeItemsCreator = _MazeItemsCreator;
            ModelData = _ModelData;
            ContainersGetter = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
            GameTimeProvider = _GameTimeProvider;

            GameTicker.Register(this);
        }

        #endregion
        
        #region api
        
        public event NoArgsHandler GameLoopUpdate;
        public event NoArgsHandler Initialized;
        public abstract List<IViewMazeItem> MazeItems { get; }


        public virtual void Init()
        {
            ContainersGetter.MazeItemsContainer.SetLocalPosXY(Vector2.zero);
            ContainersGetter.MazeItemsContainer.PlusLocalPosY(CoordinateConverter.GetScale() * 0.5f);
            Initialized?.Invoke();
        }

        public virtual void UpdateTick()
        {
            GameLoopUpdate?.Invoke();
        }

        public abstract IViewMazeItem GetItem(MazeItem _Item);
        public virtual T GetItem<T>(MazeItem _Item)  where T : IViewMazeItem => (T) GetItem(_Item);

        #endregion


    }
}