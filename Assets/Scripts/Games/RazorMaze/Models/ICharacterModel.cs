using Entities;
using UnityEngine.EventSystems;

namespace Games.RazorMaze.Models
{
    public delegate void HealthPointsChangedHandler(HealthPointsEventArgs _Args);
    public delegate void CharacterMovingHandler(float _Progress);

    public interface ICharacterModel
    {
        event V2IntV2IntHandler StartMove;
        event CharacterMovingHandler Moving;
        event NoArgsHandler FinishMove;
        event HealthPointsChangedHandler HealthChanged;
        event NoArgsHandler Death;
        void Init();
        long HealthPoints { get; set; }
        V2Int Position { get; }
        void Move(MoveDirection _Direction);
        void UpdateMazeInfo(MazeInfo _Info, MazeOrientation _Orientation);
    }
}