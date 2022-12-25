using System.Collections.Generic;
using Common.Entities;
using mazing.common.Runtime.Entities;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;

namespace RMAZOR.Models
{
    public interface IModelCharacterPositionValidator
    {
        bool IsNextPositionValid(
            IReadOnlyList<IMazeItemProceedInfo> _ProceedInfos,
            IReadOnlyList<V2Int>                _PathItems,
            V2Int                               _From,
            V2Int                               _CurrentPosition,
            V2Int                               _NextPosition,
            out V2Int?                          _BlockPositionWhoStopped);
    }
    
    public class ModelCharacterPositionValidator : IModelCharacterPositionValidator
    {
        public bool IsNextPositionValid(
            IReadOnlyList<IMazeItemProceedInfo> _ProceedInfos,
            IReadOnlyList<V2Int>                _PathItems,
            V2Int                               _From,
            V2Int                               _CurrentPosition,
            V2Int                               _NextPosition,
            out V2Int?                          _BlockPositionWhoStopped)
        {
            _BlockPositionWhoStopped = null;
            if (!IsPathNodeOnPosition(_NextPosition, _PathItems))
                return IsPortalOnPosition(_NextPosition, _ProceedInfos);
            IMazeItemProceedInfo shredinger = ShredingerOnPosition(_NextPosition, _ProceedInfos);
            if (shredinger != null)
            {
                _BlockPositionWhoStopped = _NextPosition;
                return shredinger.ProceedingStage == ModelCommonData.StageIdle;
            }
            IMazeItemProceedInfo diode = DiodeOnPosition(_NextPosition, _ProceedInfos);
            if (diode != null)
            {
                IMazeItemProceedInfo gravityBlock     = GravityBlockOnPosition(_NextPosition, _ProceedInfos);
                if (gravityBlock != null)
                    return false;
                IMazeItemProceedInfo gravityBlockFree = GravityBlockFreeOnPosition(_NextPosition, _ProceedInfos);
                if (gravityBlockFree != null)
                    return false;
                _BlockPositionWhoStopped = _NextPosition;
                bool nextPosIsInvalid = diode.Direction == -_NextPosition + _CurrentPosition;
                return !nextPosIsInvalid;
            }
            IMazeItemProceedInfo lockItem = LockItemOnPosition(_NextPosition, _ProceedInfos);
            if (lockItem != null)
            {
                _BlockPositionWhoStopped = _NextPosition;
                bool nextPosIsInvalid = lockItem.ProceedingStage == ModelCommonData.KeyLockStage1;
                return !nextPosIsInvalid;
            }
            if (IsOtherMazeItemOnPosition(_NextPosition, _ProceedInfos))
                return false;
            if (IsBusyMazeItemOnPosition(_NextPosition, _ProceedInfos))
                return false;
            if (IsSpringboardOnPosition(_CurrentPosition, _From, _ProceedInfos))
                return false;
            bool isPrevPortal = IsPortalOnPosition(_CurrentPosition, _ProceedInfos);
            bool isStartFromPortal = IsStartFromPortal(_From, _ProceedInfos);
            return !isPrevPortal || isStartFromPortal;
        }

        private static bool IsPathNodeOnPosition(
            V2Int                _Position,
            IReadOnlyList<V2Int> _PathItems)
        {
            bool isNode = false;
            for (int i = 0; i < _PathItems.Count; i++)
            {
                if (_PathItems[i] != _Position)
                    continue;
                isNode = true;
                break;
            }
            return isNode;
        }

        private static IMazeItemProceedInfo ShredingerOnPosition(
            V2Int                               _Position,
            IReadOnlyList<IMazeItemProceedInfo> _ProceedInfos)
        {
            IMazeItemProceedInfo shredinger = null;
            for (int i = 0; i < _ProceedInfos.Count; i++)
            {
                var info = _ProceedInfos[i];
                if (info.Type != EMazeItemType.ShredingerBlock)
                    continue;
                if (info.CurrentPosition != _Position)
                    continue;
                shredinger = info;
                break;
            }
            return shredinger;
        }
        
        private static IMazeItemProceedInfo DiodeOnPosition(
            V2Int                               _Position,
            IReadOnlyList<IMazeItemProceedInfo> _ProceedInfos)
        {
            IMazeItemProceedInfo diode = null;
            for (int i = 0; i < _ProceedInfos.Count; i++)
            {
                var info = _ProceedInfos[i];
                if (info.Type != EMazeItemType.Diode)
                    continue;
                if (info.CurrentPosition != _Position)
                    continue;
                diode = info;
                break;
            }
            return diode;
        }

        private static IMazeItemProceedInfo GravityBlockOnPosition(
            V2Int                               _Position,
            IReadOnlyList<IMazeItemProceedInfo> _ProceedInfos)
        {
            IMazeItemProceedInfo gravityBlock = null;
            for (int i = 0; i < _ProceedInfos.Count; i++)
            {
                var info = _ProceedInfos[i];
                if (info.Type != EMazeItemType.GravityBlock)
                    continue;
                if (info.CurrentPosition != _Position)
                    continue;
                gravityBlock = info;
                break;
            }
            return gravityBlock;
        }
        
        private static IMazeItemProceedInfo GravityBlockFreeOnPosition(
            V2Int                               _Position,
            IReadOnlyList<IMazeItemProceedInfo> _ProceedInfos)
        {
            IMazeItemProceedInfo gravityBlockFree = null;
            for (int i = 0; i < _ProceedInfos.Count; i++)
            {
                var info = _ProceedInfos[i];
                if (info.Type != EMazeItemType.GravityBlockFree)
                    continue;
                if (info.CurrentPosition != _Position)
                    continue;
                gravityBlockFree = info;
                break;
            }
            return gravityBlockFree;
        }

        private static IMazeItemProceedInfo LockItemOnPosition(
            V2Int                               _Position,
            IReadOnlyList<IMazeItemProceedInfo> _ProceedInfos)
        {
            IMazeItemProceedInfo lockItem = null;
            for (int i = 0; i < _ProceedInfos.Count; i++)
            {
                var info = _ProceedInfos[i];
                if (info.Type != EMazeItemType.KeyLock)
                    continue;
                if (info.CurrentPosition != _Position)
                    continue;
                if (info.Direction != V2Int.Right)
                    continue;
                lockItem = info;
                break;
            }
            return lockItem;
        }

        private static bool IsOtherMazeItemOnPosition(
            V2Int                               _NextPosition,
            IReadOnlyList<IMazeItemProceedInfo> _ProceedInfos)
        {
            bool isMazeItem = false;
            for (int i = 0; i < _ProceedInfos.Count; i++)
            {
                var info = _ProceedInfos[i];
                if (info.CurrentPosition != _NextPosition)
                    continue;
                if (info.Type != EMazeItemType.Block 
                    && info.Type != EMazeItemType.TrapIncreasing
                    && info.Type != EMazeItemType.Turret)
                    continue;
                isMazeItem = true;
                break;
            }
            return isMazeItem;
        }
        
        private static bool IsBusyMazeItemOnPosition(
            V2Int                               _NextPosition,
            IReadOnlyList<IMazeItemProceedInfo> _ProceedInfos)
        {
            bool isBuzyMazeItem = false;
            for (int i = 0; i < _ProceedInfos.Count; i++)
            {
                var info = _ProceedInfos[i];
                if (info.Type != EMazeItemType.GravityBlock && info.Type != EMazeItemType.GravityBlockFree)
                    continue;
                bool busyPositionsContainNext = false;
                for (int j = 0; j < info.BusyPositions.Count; j++)
                {
                    if (info.BusyPositions[j] != _NextPosition)
                        continue;
                    busyPositionsContainNext = true;
                    break;
                }
                if (!busyPositionsContainNext)
                    continue;
                isBuzyMazeItem = true;
                break;
            }
            return isBuzyMazeItem;
        }

        private static bool IsSpringboardOnPosition(
            V2Int                               _Position,
            V2Int                               _From,
            IReadOnlyList<IMazeItemProceedInfo> _ProceedInfos)
        {
            if (_Position == _From)
                return false;
            bool isSpringboard = false;
            for (int i = 0; i < _ProceedInfos.Count; i++)
            {
                var info = _ProceedInfos[i];
                if (info.CurrentPosition != _Position)
                    continue;
                if (info.Type != EMazeItemType.Springboard)
                    continue;
                isSpringboard = true;
                break;
            }
            return isSpringboard;
        }

        private static bool IsPortalOnPosition(
            V2Int                               _Position,
            IReadOnlyList<IMazeItemProceedInfo> _ProceedInfos)
        {
            bool isPortal = false;
            for (int i = 0; i < _ProceedInfos.Count; i++)
            {
                var info = _ProceedInfos[i];
                if (info.CurrentPosition != _Position)
                    continue;
                if (info.Type != EMazeItemType.Portal)
                    continue;
                isPortal = true;
                break;
            }
            return isPortal;
        }

        private static bool IsStartFromPortal(
            V2Int                               _From,
            IReadOnlyList<IMazeItemProceedInfo> _ProceedInfos)
        {
            bool isStartFromPortal = false;
            for (int i = 0; i < _ProceedInfos.Count; i++)
            {
                var info = _ProceedInfos[i];
                if (info.CurrentPosition != _From)
                    continue;
                if (info.Type != EMazeItemType.Portal)
                    continue;
                isStartFromPortal = true;
                break;
            }
            return isStartFromPortal;
        }
    }
}