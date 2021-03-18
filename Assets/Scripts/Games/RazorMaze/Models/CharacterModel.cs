using System;
using System.Linq;
using Entities;
using Games.RazorMaze.Models.Obstacles;
using UnityEngine.EventSystems;
using Utils;

namespace Games.RazorMaze.Models
{
    public class CharacterModel : ICharacterModel
    {
        #region nonpublic members

        private MazeInfo m_MazeInfo;
        private MazeOrientation m_Orientation;
        private long m_HealthPoints;
        
        #endregion
        
        #region api

        public event V2IntV2IntHandler StartMove;
        public event CharacterMovingHandler Moving;
        public event NoArgsHandler FinishMove;
        public event HealthPointsChangedHandler HealthChanged;
        public event NoArgsHandler Death;

        public V2Int Position { get; private set; }
        
        public long HealthPoints
        {
            get => m_HealthPoints;
            set
            {
                m_HealthPoints = Math.Max(0, value);
                HealthChanged?.Invoke(new HealthPointsEventArgs(m_HealthPoints));
                if (m_HealthPoints <= 0)
                    Death?.Invoke();
            }
        }

        public void Init()
        {
            HealthPoints = 1;
        }
        
        public void Move(MoveDirection _Direction)
        {
            var prevPos = Position;
            Position = GetNewPosition(_Direction);
            StartMove?.Invoke(prevPos, Position);
            Coroutines.Run(Coroutines.Lerp(
                0f,
                1f,
                0.1f,
                _Progress => Moving?.Invoke(_Progress),
                GameTimeProvider.Instance,
                () => FinishMove?.Invoke()));
        }

        public void UpdateMazeInfo(MazeInfo _Info, MazeOrientation _Orientation)
        {
            if (!ReferenceEquals(m_MazeInfo, _Info))
            {
                m_MazeInfo = _Info;
                Position = m_MazeInfo.Nodes[0].Position;
            }
            m_Orientation = _Orientation;
        }

        #endregion
        
        #region nonpublic methods

        private V2Int GetNewPosition(MoveDirection _Direction)
        {
            var nextPos = Position;
            var dirVector = GetDirectionVector(_Direction);
            while (ValidPosition(nextPos + dirVector, m_MazeInfo))
                nextPos += dirVector;
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
            switch (m_Orientation)
            {
                case MazeOrientation.North:
                    switch (_Direction)
                    {
                        case MoveDirection.Up:    return V2Int.up;
                        case MoveDirection.Right: return V2Int.right;
                        case MoveDirection.Down:  return V2Int.down;
                        case MoveDirection.Left:  return V2Int.left;
                    } break;
                case MazeOrientation.East:
                    switch (_Direction)
                    {
                        case MoveDirection.Up:    return V2Int.left;
                        case MoveDirection.Right: return V2Int.up;
                        case MoveDirection.Down:  return V2Int.right;
                        case MoveDirection.Left:  return V2Int.down;
                    } break;
                case MazeOrientation.South:
                    switch (_Direction)
                    {
                        case MoveDirection.Up:    return V2Int.down;
                        case MoveDirection.Right: return V2Int.left;
                        case MoveDirection.Down:  return V2Int.up;
                        case MoveDirection.Left:  return V2Int.right;
                    } break;
                case MazeOrientation.West:
                    switch (_Direction)
                    {
                        case MoveDirection.Up:    return V2Int.right;
                        case MoveDirection.Right: return V2Int.down;
                        case MoveDirection.Down:  return V2Int.left;
                        case MoveDirection.Left:  return V2Int.up;
                    } break;
            }
            return default;
        }

        #endregion
    }
}