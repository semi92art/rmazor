using Extensions;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views
{
    public interface IContainersGetter
    {
        Transform MazeContainer { get; }
        Transform MazeItemsContainer { get; }
        Transform CharacterContainer { get; }
    }

    public class ContainersGetter : IContainersGetter
    {
        private ICoordinateConverter CoordinateConverter { get; }

        public Transform MazeContainer { get; }

        public Transform MazeItemsContainer { get; }
        public Transform CharacterContainer { get; }

        public ContainersGetter(ICoordinateConverter _CoordinateConverter)
        {
            CoordinateConverter = _CoordinateConverter;
            MazeContainer = CommonUtils.FindOrCreateGameObject("Maze", out _).transform;
            MazeContainer.SetPosXY(CoordinateConverter.GetCenter());
            MazeItemsContainer = CommonUtils.FindOrCreateGameObject("Maze Items", out _).transform;
            MazeItemsContainer.SetParent(MazeContainer);
            CharacterContainer = CommonUtils.FindOrCreateGameObject("Character", out _).transform;
            CharacterContainer.SetParent(MazeContainer);
        }
    }
}