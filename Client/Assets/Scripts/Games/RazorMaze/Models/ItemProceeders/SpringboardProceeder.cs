using System;
using Entities;
using Games.RazorMaze.Models.ProceedInfos;
using Ticker;

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

    public interface ISpringboardProceeder : IItemsProceeder, ICharacterMoveContinued, ICharacterMoveFinished
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

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            ProceedSpringboardIfOnPosition(_Args.Position, _Args.Direction, false);
        }
        
        #endregion

        public void OnCharacterMoveFinished(CharacterMovingEventArgs _Args)
        {
            ProceedSpringboardIfOnPosition(_Args.To, _Args.Direction, true);
        }

        private void ProceedSpringboardIfOnPosition(V2Int _Position, EMazeMoveDirection _Direction, bool _Forced)
        {
            IMazeItemProceedInfo info = null;
            for (int i = 0; i < ProceedInfos.Length; i++)
            {
                if (ProceedInfos[i].CurrentPosition != _Position)
                    continue;
                info = ProceedInfos[i];
                break;
            }
            if (info == null)
            {
                m_LastArgs = null;
                return;
            }
            if (m_LastArgs != null && !_Forced)
                return;
            var charInverseDir = -1 * RazorMazeUtils.GetDirectionVector(_Direction, Data.Orientation);
            V2Int newDirection = default;
            if (info.Direction == V2Int.up + V2Int.left)
                newDirection = charInverseDir == V2Int.up ? V2Int.left : V2Int.up;
            else if (info.Direction == V2Int.up + V2Int.right)
                newDirection = charInverseDir == V2Int.up ? V2Int.right : V2Int.up;
            else if (info.Direction == V2Int.down + V2Int.left)
                newDirection = charInverseDir == V2Int.down ? V2Int.left : V2Int.down;
            else if (info.Direction == V2Int.down + V2Int.right)
                newDirection = charInverseDir == V2Int.down ? V2Int.right : V2Int.down;
            var moveDirection = RazorMazeUtils.GetMoveDirection(newDirection, Data.Orientation);
            m_LastArgs = new SpringboardEventArgs(moveDirection, info);
            SpringboardEvent?.Invoke(m_LastArgs);
        }
    }
}