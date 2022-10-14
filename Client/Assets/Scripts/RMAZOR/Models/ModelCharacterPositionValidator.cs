using System.Collections.Generic;
using Common.Entities;
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
            bool isNode = false;
            for (int i = 0; i < _PathItems.Count; i++)
            {
                if (_PathItems[i] != _NextPosition)
                    continue;
                isNode = true;
                break;
            }
            if (!isNode)
            {
                for (int i = 0; i < _ProceedInfos.Count; i++)
                {
                    var info = _ProceedInfos[i];
                    if (info.CurrentPosition != _NextPosition)
                        continue;
                    if (info.Type != EMazeItemType.Portal)
                        continue;
                    return true;
                }
                return false;
            }
            IMazeItemProceedInfo shredinger = null;
            for (int i = 0; i < _ProceedInfos.Count; i++)
            {
                var info = _ProceedInfos[i];
                if (info.Type != EMazeItemType.ShredingerBlock)
                    continue;
                if (info.CurrentPosition != _NextPosition)
                    continue;
                shredinger = info;
                break;
            }
            if (shredinger != null)
            {
                _BlockPositionWhoStopped = _NextPosition;
                return shredinger.ProceedingStage == ModelCommonData.StageIdle;
            }
            IMazeItemProceedInfo diode = null;
            for (int i = 0; i < _ProceedInfos.Count; i++)
            {
                var info = _ProceedInfos[i];
                if (info.Type != EMazeItemType.Diode)
                    continue;
                if (info.CurrentPosition != _NextPosition)
                    continue;
                diode = info;
                break;
            }
            if (diode != null)
            {
                _BlockPositionWhoStopped = _NextPosition;
                bool nextPosIsInvalid = diode.Direction == -_NextPosition + _CurrentPosition;
                return !nextPosIsInvalid;
            }
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
            if (isMazeItem)
                return false;
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
            if (isBuzyMazeItem)
                return false;
            bool isPrevSpringboard = false;
            if (_CurrentPosition != _From)
            {
                for (int i = 0; i < _ProceedInfos.Count; i++)
                {
                    var info = _ProceedInfos[i];
                    if (info.CurrentPosition != _CurrentPosition)
                        continue;
                    if (info.Type != EMazeItemType.Springboard)
                        continue;
                    isPrevSpringboard = true;
                    break;
                }
            }
            if (isPrevSpringboard)
                return false;
            bool isPrevPortal = false;
            for (int i = 0; i < _ProceedInfos.Count; i++)
            {
                var info = _ProceedInfos[i];
                if (info.CurrentPosition != _CurrentPosition)
                    continue;
                if (info.Type != EMazeItemType.Portal)
                    continue;
                isPrevPortal = true;
                break;
            }
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
            return !isPrevPortal || isStartFromPortal;
        }
    }
}