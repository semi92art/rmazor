using Entities;
using Games.RazorMaze.Models;

namespace Games.RazorMaze.Views
{
    public interface ICharacterView
    {
        void Init();
        void OnStartChangePosition(V2Int _PrevPos, V2Int _NextPos);
        void OnMoving(float _Progress);
        void OnDeath();
        void OnHealthChanged(HealthPointsEventArgs _Args);
    }
}