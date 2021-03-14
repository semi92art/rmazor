using System;
using System.Linq;
using Entities;
using Exceptions;
using Games.RazorMaze.Models.Obstacles;
using UnityEngine.EventSystems;
using Utils;

namespace Games.RazorMaze.Models
{
    public class CharacherModelDefault : ICharacterModel
    {
        #region nonpublic members

        private MazeInfo m_MazeInfo;
        private long m_HealthPoints;
        
        #endregion
        
        #region api

        public event V2IntV2IntHandler OnStartChangePosition;
        public event CharacterMovingHandler OnMoving;
        public event HealthPointsChangedHandler OnHealthChanged;
        public event NoArgsHandler OnDeath;

        public V2Int Position { get; private set; }
        
        public long HealthPoints
        {
            get => m_HealthPoints;
            set
            {
                m_HealthPoints = Math.Max(0, value);
                OnHealthChanged?.Invoke(new HealthPointsEventArgs(m_HealthPoints));
                if (m_HealthPoints <= 0)
                    OnDeath?.Invoke();
            }
        }

        public void Init(HealthPointsEventArgs _HealthPointArgs)
        {
            HealthPoints = _HealthPointArgs.HealthPoints;
        }
        
        public void Move(MoveDirection _Direction)
        {
            var prevPos = Position;
            Position = GetNewPosition(_Direction);
            OnStartChangePosition?.Invoke(prevPos, Position);
            Coroutines.Run(Coroutines.Lerp(
                0f,
                1f,
                0.3f,
                _Progress => OnMoving?.Invoke(_Progress),
                GameTimeProvider.Instance));
        }

        public void UpdateMazeInfo(MazeInfo _Info)
        {
            m_MazeInfo = _Info;
            Position = m_MazeInfo.Nodes[0].Position;
        }

        #endregion
        
        #region nonpublic methods

        private V2Int GetNewPosition(MoveDirection _Direction)
        {
            var nextPos = Position;
            var dirVector = GetDirectionVector(_Direction);
            do
            {
                nextPos += dirVector;
            }
            while (ValidPosition(nextPos + dirVector, m_MazeInfo));
            return nextPos;
        }

        private bool ValidPosition(V2Int _Position, MazeInfo _Info)
        {
            return _Info.Nodes.Any(_N => _N.Position == _Position)
                   && _Info.MovingItems.Where(_Item => _Item.Type == MovingItemType.Block)
                       .All(_Item => _Item.Position != _Position);
        }

        private V2Int GetDirectionVector(MoveDirection _Direction)
        {
            switch (_Direction)
            {
                case MoveDirection.Up:    return V2Int.up;
                case MoveDirection.Down:  return V2Int.down;
                case MoveDirection.Left:  return V2Int.left;
                case MoveDirection.Right: return V2Int.right;
                default: throw new SwitchCaseNotImplementedException(_Direction);
            }
        }
        
        #endregion
    }
}