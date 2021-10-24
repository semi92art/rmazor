using UnityEngine;

namespace Games.RazorMaze.Views.ContainerGetters
{
    public interface IContainersGetter
    {
        Transform GetContainer(string _Name);
    }
}