using System;
using System.Linq;
using Entities;

namespace Games.RazorMaze.Models
{
    public class CharacterModel : ICharacterModelFull
    {

        #region nonpublic members

        private MazeInfo m_MazeInfo;
        private MazeOrientation m_Orientation;
        private long m_HealthPoints;
        
        #endregion
        
        #region inject
        private ICharacterMover CharacterMover { get; }
        private IMazeTransformer Transformer { get; }

        public CharacterModel(ICharacterMover _CharacterMover, IMazeTransformer _Transformer)
        {
            CharacterMover = _CharacterMover;
            Transformer = _Transformer;
            CharacterMover.CharacterMoveStarted += _Progress => MoveStarted?.Invoke(_Progress);
            CharacterMover.CharacterMoveContinued += _Progress => MoveContinued?.Invoke(_Progress);
            CharacterMover.CharacterMoveFinished += _Position => MoveFinished?.Invoke(_Position);
        }
        
        #endregion
        
        #region api

        public event CharacterMovingHandler MoveStarted;
        public event CharacterMovingHandler MoveContinued;
        public event CharacterMovingHandler MoveFinished;
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
        
        public void Move(MazeMoveDirection _Direction)
        {
            var prevPos = Position;
            Position = GetNewPosition(_Direction);
            CharacterMover.MoveCharacter(prevPos, Position);
        }

        public void OnMazeInfoUpdated(MazeInfo _Info, MazeOrientation _Orientation)
        {
            if (!ReferenceEquals(m_MazeInfo, _Info))
            {
                m_MazeInfo = _Info;
                Position = m_MazeInfo.Path[0];
            }
            m_Orientation = _Orientation;
        }

        #endregion
        
        #region nonpublic methods

        private V2Int GetNewPosition(MazeMoveDirection _Direction)
        {
            var nextPos = Position;
            var dirVector = RazorMazeUtils.GetDirectionVector(_Direction, m_Orientation);
            while (ValidPosition(nextPos + dirVector, m_MazeInfo))
                nextPos += dirVector;
            return nextPos;
        }

        private bool ValidPosition(V2Int _Position, MazeInfo _Info)
        {
            bool isNode = _Info.Path.Any(_PathItem => _PathItem == _Position);
            bool isMazeItem = _Info.MazeItems.Any(_O => 
                _O.Position == _Position && _O.Type == EMazeItemType.Block);
            bool isBuzyMazeItem = Transformer.GravityProceeds.Values
                .Where(_Proceed => _Proceed.Item.Type == EMazeItemType.BlockMovingGravity)
                .Any(_Proceed => _Proceed.BusyPositions.Contains(_Position));
            return isNode && !isMazeItem && !isBuzyMazeItem;
        }
        
        #endregion
    }
}