using Games.RazorMaze.Models;

namespace Games.RazorMaze.Views.Characters
{
    public interface IViewCharacterTail
    {
        void Init();
        void ShowTail(CharacterMovingEventArgs _Args);
        void HideTail(CharacterMovingEventArgs _Args);
    }
}