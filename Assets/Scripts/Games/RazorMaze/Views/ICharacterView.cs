using Entities;
using Games.RazorMaze.Models;

namespace Games.RazorMaze.Views
{
    public interface ICharacterView
    {
        void Init();
        void OnStartChangePosition(CharacterMovingEventArgs _Args);
        void OnMoving(CharacterMovingEventArgs _Args);
        void OnDeath();
        void OnHealthChanged(HealthPointsEventArgs _Args);
    }
}