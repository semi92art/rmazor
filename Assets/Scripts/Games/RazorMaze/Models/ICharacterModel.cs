using System;
using Entities;

namespace Games.RazorMaze.Models
{
    public class CharacterMovingEventArgs : EventArgs
    {
        public V2Int From { get; }
        public V2Int To { get; }
        public float Progress { get; }

        public CharacterMovingEventArgs(V2Int _From, V2Int _To, float _Progress)
        {
            From = _From;
            To = _To;
            Progress = _Progress;
        }
    }
    
    public delegate void HealthPointsChangedHandler(HealthPointsEventArgs _Args);
    public delegate void CharacterMovingHandler(CharacterMovingEventArgs _Args);

    public interface ICharacterModel
    {
        event CharacterMovingHandler MoveStarted;
        event CharacterMovingHandler MoveContinued;
        event CharacterMovingHandler MoveFinished;
        event HealthPointsChangedHandler HealthChanged;
        event NoArgsHandler Death;
        void Init();
        long HealthPoints { get; set; }
        V2Int Position { get; }
        void Move(MazeMoveDirection _Direction);
    }

    public interface ICharacterModelFull : ICharacterModel
    {
        void OnMazeInfoUpdated(MazeInfo _Info, MazeOrientation _Orientation);
    }
}