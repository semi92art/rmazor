using Games.RazorMaze.Models;
using SpawnPools;

namespace Games.RazorMaze.Views.Characters
{
    public interface IViewCharacterTail : IActivated
    {
        void ShowTail(CharacterMoveEventArgsBase _Args);
        void HideTail(CharacterMovingFinishedEventArgs _Args = null);
    }
}