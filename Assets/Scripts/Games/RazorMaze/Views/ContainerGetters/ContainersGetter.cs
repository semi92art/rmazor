using Extensions;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.ContainerGetters
{
    public class ContainersGetter : IContainersGetter
    {
        #region inject
        
        private ICoordinateConverter CoordinateConverter { get; }

        public ContainersGetter(ICoordinateConverter _CoordinateConverter)
        {
            CoordinateConverter = _CoordinateConverter;
            MazeContainer = CommonUtils.FindOrCreateGameObject("Maze", out _).transform;
            MazeContainer.SetPosXY(CoordinateConverter.GetCenter());
            MazeItemsContainer = CommonUtils.FindOrCreateGameObject("Maze Items", out _).transform;
            MazeItemsContainer.SetParent(MazeContainer);
            CharacterContainer = CommonUtils.FindOrCreateGameObject("Character", out _).transform;
            CharacterContainer.SetParent(MazeContainer);
            MazeItemsContainer.SetPosXY(_CoordinateConverter.GetCenter());
        }
        
        #endregion

        #region api

        public Transform MazeContainer { get; }
        public Transform MazeItemsContainer { get; }
        public Transform CharacterContainer { get; }

        #endregion
    }
}