using Games.RazorMaze.Models;

namespace Games.RazorMaze.Views.Characters
{
    public interface IViewCharacter : IInit
    {
        void OnStartChangePosition(CharacterMovingEventArgs _Args);
        void OnMoving(CharacterMovingEventArgs _Args);
        void OnDeath();
        void OnHealthChanged(HealthPointsEventArgs _Args);
    }
}