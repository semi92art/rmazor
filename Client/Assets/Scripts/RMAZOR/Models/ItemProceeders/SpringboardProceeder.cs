using System;
using Common.Entities;
using Common.Ticker;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;

namespace RMAZOR.Models.ItemProceeders
{
    public class SpringboardEventArgs : EventArgs
    {
        public EMazeMoveDirection Direction { get; }
        public IMazeItemProceedInfo Info { get; }

        public SpringboardEventArgs(EMazeMoveDirection _Direction, IMazeItemProceedInfo _Info)
        {
            Direction = _Direction;
            Info = _Info;
        }
    }

    public delegate void SpringboardEventHandler(SpringboardEventArgs _Args);

    public interface ISpringboardProceeder : IItemsProceeder, ICharacterMoveFinished
    {
        event SpringboardEventHandler SpringboardEvent;
    }
    
    public class SpringboardProceeder : ItemsProceederBase, ISpringboardProceeder
    {
        #region nonpbulic members

        private SpringboardEventArgs m_LastArgs;

        #endregion
        
        #region inject
        
        private SpringboardProceeder(
            ModelSettings    _Settings,
            IModelData       _Data,
            IModelCharacter  _Character,
            IModelGameTicker _GameTicker,
            IModelMazeRotation _Rotation)
            : base (_Settings, _Data, _Character, _GameTicker, _Rotation) { }
        
        #endregion
        
        #region api

        protected override EMazeItemType[] Types => new[] {EMazeItemType.Springboard};
        public event SpringboardEventHandler SpringboardEvent;

        #endregion

        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            ProceedSpringboardIfOnPosition(_Args.BlockOnFinish, _Args.Direction, true);
        }

        private void ProceedSpringboardIfOnPosition(
            IMazeItemProceedInfo _Info,
            EMazeMoveDirection _Direction,
            bool _Forced)
        {
            if (_Info == null || _Info.Type != EMazeItemType.Springboard)
            {
                m_LastArgs = null;
                return;
            }
            if (m_LastArgs != null && !_Forced)
                return;
            var charInverseDir = -1 * RmazorUtils.GetDirectionVector(_Direction, Rotation.Orientation);
            V2Int newDirection = default;
            if (_Info.Direction == V2Int.Up + V2Int.Left)
                newDirection = charInverseDir == V2Int.Up ? V2Int.Left : V2Int.Up;
            else if (_Info.Direction == V2Int.Up + V2Int.Right)
                newDirection = charInverseDir == V2Int.Up ? V2Int.Right : V2Int.Up;
            else if (_Info.Direction == V2Int.Down + V2Int.Left)
                newDirection = charInverseDir == V2Int.Down ? V2Int.Left : V2Int.Down;
            else if (_Info.Direction == V2Int.Down + V2Int.Right)
                newDirection = charInverseDir == V2Int.Down ? V2Int.Right : V2Int.Down;
            var moveDirection = RmazorUtils.GetMoveDirection(newDirection, Rotation.Orientation);
            m_LastArgs = new SpringboardEventArgs(moveDirection, _Info);
            SpringboardEvent?.Invoke(m_LastArgs);
        }
    }
}