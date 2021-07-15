using Games.RazorMaze.Models;

namespace Games.RazorMaze.Views.Characters
{
    public interface IViewCharacter : IInit
    {
        void OnMovingStarted(CharacterMovingEventArgs _Args);
        void OnMoving(CharacterMovingEventArgs _Args);
        void OnMovingFinished(CharacterMovingEventArgs _Args);
        void OnDeath();
        void OnHealthChanged(HealthPointsEventArgs _Args);
    }
}