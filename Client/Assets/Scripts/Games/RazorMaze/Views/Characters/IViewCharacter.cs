using Entities;
using Games.RazorMaze.Models;
using SpawnPools;

namespace Games.RazorMaze.Views.Characters
{
    public interface IViewCharacter : IInit, IActivated
    {
        void OnMovingStarted(CharacterMovingEventArgs _Args);
        void OnMoving(CharacterMovingEventArgs _Args);
        void OnMovingFinished(CharacterMovingEventArgs _Args);
        void OnPositionSet(V2Int _Position);
        void OnAliveOrDeath(bool _Alive);
    }
}