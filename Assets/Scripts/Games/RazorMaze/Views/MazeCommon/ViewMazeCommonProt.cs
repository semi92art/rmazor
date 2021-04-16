using System.Collections.Generic;
using System.Linq;
using Entities;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.MazeItems;
using Games.RazorMaze.Views.Utils;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeCommon
{
    public class ViewMazeCommonProt : IViewMazeCommon
    {
        #region inject

        public IMazeItemsCreator MazeItemsCreator { get; }
        private IModelMazeData Model { get; }
        private IContainersGetter ContainersGetter { get; }
        private ICoordinateConverter CoordinateConverter { get; }

        public ViewMazeCommonProt(
            IMazeItemsCreator _MazeItemsCreator,
            IModelMazeData _Model,
            IContainersGetter _ContainersGetter, 
            ICoordinateConverter _CoordinateConverter)
        {
            MazeItemsCreator = _MazeItemsCreator;
            Model = _Model;
            ContainersGetter = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;

            Camera.main.backgroundColor = ViewUtils.ColorMain;
        }

        #endregion

        #region api
        
        public List<IViewMazeItem> MazeItems { get; private set; }
        
        public void OnPathProceed(V2Int _PathItem)
        {
            var item = MazeItems.First(_Item => _Item.Props.Position == _PathItem && _Item.Props.IsNode);
            item.Active = true;
        }

        public IViewMazeItem GetItem(MazeItem _Item)
        {
            return MazeItems.SingleOrDefault(_Itm => _Itm.Equal(_Item));
        }

        public T GetItem<T>(MazeItem _Item) where T : IViewMazeItem
        {
            var item = MazeItems.SingleOrDefault(_Itm => _Itm.Equal(_Item));
            return (T) item;
        }

        public void Init()
        {
            MazeItems = MazeItemsCreator.CreateMazeItems(Model.Info);
            MazeItems.First(_Item => _Item.Props.IsStartNode).Active = true;
            ContainersGetter.MazeItemsContainer.SetLocalPosXY(Vector2.zero);
            ContainersGetter.MazeItemsContainer.PlusLocalPosY(CoordinateConverter.GetScale() * 0.5f);
        }
        
        #endregion
    }
}