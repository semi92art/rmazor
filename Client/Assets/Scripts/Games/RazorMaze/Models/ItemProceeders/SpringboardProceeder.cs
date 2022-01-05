using System;
using Entities;
using Games.RazorMaze.Models.ProceedInfos;
using Ticker;
using Utils;

namespace Games.RazorMaze.Models.ItemProceeders
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
        
        public SpringboardProceeder(
            ModelSettings _Settings,
            IModelData _Data,
            IModelCharacter _Character,
            IModelLevelStaging _LevelStaging,
            IModelGameTicker _GameTicker)
            : base (_Settings, _Data, _Character, _LevelStaging, _GameTicker) { }
        
        #endregion
        
        #region api
        
        public override EMazeItemType[] Types => new[] {EMazeItemType.Springboard};
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
            var charInverseDir = -1 * RazorMazeUtils.GetDirectionVector(_Direction, Data.Orientation);
            V2Int newDirection = default;
            if (_Info.Direction == V2Int.up + V2Int.left)
                newDirection = charInverseDir == V2Int.up ? V2Int.left : V2Int.up;
            else if (_Info.Direction == V2Int.up + V2Int.right)
                newDirection = charInverseDir == V2Int.up ? V2Int.right : V2Int.up;
            else if (_Info.Direction == V2Int.down + V2Int.left)
                newDirection = charInverseDir == V2Int.down ? V2Int.left : V2Int.down;
            else if (_Info.Direction == V2Int.down + V2Int.right)
                newDirection = charInverseDir == V2Int.down ? V2Int.right : V2Int.down;
            var moveDirection = RazorMazeUtils.GetMoveDirection(newDirection, Data.Orientation);
            m_LastArgs = new SpringboardEventArgs(moveDirection, _Info);
            SpringboardEvent?.Invoke(m_LastArgs);
        }
    }
}