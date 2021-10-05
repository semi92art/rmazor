using UnityEngine;

namespace Games.RazorMaze.Views.ContainerGetters
{
    public interface IContainersGetter
    {
        Transform MazeContainer { get; }
        Transform MazeItemsContainer { get; }
        Transform CharacterContainer { get; }
        Transform BackgroundContainer { get; }
        Transform AudioSourcesContainer { get; }
    }
}