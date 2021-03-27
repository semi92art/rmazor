using System.Collections.Generic;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Prot;
using Games.RazorMaze.Views.MazeItems;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeCommon
{
    public class ViewMazeCommonProt : IViewMazeCommon
    {
        #region inject

        private IModelMazeData Model { get; }
        private IContainersGetter ContainersGetter { get; }
        private ICoordinateConverter CoordinateConverter { get; }

        public ViewMazeCommonProt(IModelMazeData _Model, IContainersGetter _ContainersGetter, ICoordinateConverter _CoordinateConverter)
        {
            Model = _Model;
            ContainersGetter = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
        }

        #endregion

        #region api
        
        public List<IViewMazeItem> MazeItems { get; private set; }
        
        public void Init()
        {
            MazeItems = RazorMazePrototypingUtils.CreateMazeItems(Model.Info, ContainersGetter.MazeItemsContainer);
            CoordinateConverter.Init(Model.Info.Size);
            ContainersGetter.MazeItemsContainer.SetLocalPosXY(Vector2.zero);
            ContainersGetter.MazeItemsContainer.PlusLocalPosY(CoordinateConverter.GetScale() * 0.5f);
        }
        
        #endregion
    }
}