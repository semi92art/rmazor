using System;
using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;

namespace RMAZOR.Models
{
    public interface IRevivePositionProvider
    {
        V2Int GetLastValidPositionForRevive(
            IEnumerable<IMazeItemProceedInfo> _ProceedInfos,
            IReadOnlyList<V2Int>              _PassedPathItems);
    }
    
    public class RevivePositionProvider : IRevivePositionProvider
    {
        #region nonpublic members
        
        private List<V2Int> m_InvalidPositions;
        
        #endregion

        #region api

        public V2Int GetLastValidPositionForRevive(
            IEnumerable<IMazeItemProceedInfo> _ProceedInfos,
            IReadOnlyList<V2Int>              _PassedPathItems)
        {
            m_InvalidPositions = GetInvalidPositions(_ProceedInfos);
            var uniquePassedPathItemsInReversedOrder = _PassedPathItems.Reverse().Distinct();
            foreach (var pathItem in uniquePassedPathItemsInReversedOrder)
            {
                if (IsPositionValid(pathItem))
                    return pathItem;
            }
            return _PassedPathItems.First();
        }

        #endregion

        #region nonpublic methods

        private bool IsPositionValid(V2Int _Position)
        {
            return !m_InvalidPositions.Contains(_Position);
        }

        private static List<V2Int> GetInvalidPositions(
            IEnumerable<IMazeItemProceedInfo> _ProceedInfos)
        {
            var invalidPositions = new List<V2Int>();
            foreach (var proceedInfo in _ProceedInfos)
            {
                switch (proceedInfo.Type)
                {
                    case EMazeItemType.ShredingerBlock:
                    case EMazeItemType.Portal:
                    case EMazeItemType.GravityTrap:
                    case EMazeItemType.GravityBlockFree:
                    case EMazeItemType.Springboard:
                    case EMazeItemType.GravityBlock:
                        invalidPositions.Add(proceedInfo.CurrentPosition);
                        break;
                    case EMazeItemType.TrapReact:
                        invalidPositions.Add(GetTrapReactInvalidPosition(proceedInfo));
                        break;
                    case EMazeItemType.TrapMoving:
                        invalidPositions.AddRange(GetTrapMovingInvalidPositions(proceedInfo));
                        break;
                    case EMazeItemType.TrapIncreasing:
                        invalidPositions.AddRange(GetTrapIncreasingInvalidPositions(proceedInfo));
                        break;
                    case EMazeItemType.Hammer:
                        invalidPositions.AddRange(GetHammerInvalidPositions(proceedInfo));
                        break;
                    case EMazeItemType.Turret:
                        invalidPositions.AddRange(GetTurretInvalidPositions(proceedInfo));
                        break;
                    case EMazeItemType.Spear:
                    case EMazeItemType.Block:
                    case EMazeItemType.Diode:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return invalidPositions.Distinct().ToList();
        }

        private static V2Int GetTrapReactInvalidPosition(IMazeItemProceedInfo _ProceedInfo)
        {
            return _ProceedInfo.CurrentPosition + _ProceedInfo.Direction;
        }

        private static IEnumerable<V2Int> GetTrapMovingInvalidPositions(IMazeItemProceedInfo _ProceedInfo)
        {
            var pathPositions = new List<V2Int>();
            var joints = _ProceedInfo.Path;
            for (int i = 0; i < joints.Count - 1; i++)
            {
                var joint1 = joints[i];
                var joint2 = joints[i + 1];
                var path12 = RmazorUtils.GetFullPath(joint1, joint2);
                pathPositions.AddRange(path12);
            }
            return pathPositions.Distinct().ToList();
        }

        private static IEnumerable<V2Int> GetTrapIncreasingInvalidPositions(IMazeItemProceedInfo _ProceedInfo)
        {
            var itemPosition = _ProceedInfo.CurrentPosition;
            var invalidPositions = new List<V2Int>
            {
                itemPosition + V2Int.Left,
                itemPosition + V2Int.Right,
                itemPosition + V2Int.Down,
                itemPosition + V2Int.Up,
                itemPosition + V2Int.Left + V2Int.Down,
                itemPosition + V2Int.Left + V2Int.Up,
                itemPosition + V2Int.Right + V2Int.Down,
                itemPosition + V2Int.Right + V2Int.Up,
            };
            return invalidPositions;
        }

        private static IEnumerable<V2Int> GetHammerInvalidPositions(IMazeItemProceedInfo _ProceedInfo)
        {
            return GetTrapIncreasingInvalidPositions(_ProceedInfo);
        }

        private static IEnumerable<V2Int> GetTurretInvalidPositions(IMazeItemProceedInfo _ProceedInfo)
        {
            var itemPosition = _ProceedInfo.CurrentPosition;
            var itemDirection = _ProceedInfo.Direction;
            var invalidPositions = new List<V2Int>();
            for (int i = 1; i <= 50; i++)
                invalidPositions.Add(itemPosition + itemDirection * i);
            return invalidPositions;
        }

        #endregion
    }
}