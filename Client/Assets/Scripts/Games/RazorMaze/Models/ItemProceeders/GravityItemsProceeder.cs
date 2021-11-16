using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Exceptions;
using Games.RazorMaze.Models.ProceedInfos;
using Ticker;
using Utils;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public interface IGravityItemsProceeder : IMovingItemsProceeder, ICharacterMoveStarted
    {
        void OnShredingerBlockEvent(ShredingerBlockArgs _Args);
        void OnMazeOrientationChanged();
    }
    
    public class GravityItemsProceeder : 
        MovingItemsProceederBase, 
        IGravityItemsProceeder, 
        IGetAllProceedInfos
    {
        #region constants

        public const int StageDrop = 1;
        
        #endregion
        
        #region inject
        
        private IPathItemsProceeder PathItemsProceeder { get; }
        
        public GravityItemsProceeder(
            ModelSettings _Settings,
            IModelData _Data, 
            IModelCharacter _Character,
            IModelLevelStaging _LevelStaging,
            IModelGameTicker _GameTicker,
            IPathItemsProceeder _PathItemsProceeder) 
            : base (_Settings, _Data, _Character, _LevelStaging, _GameTicker)
        {
            PathItemsProceeder = _PathItemsProceeder;
        }
        
        #endregion
        
        #region api

        public override EMazeItemType[] Types => RazorMazeUtils.GravityItemTypes();

        public Func<List<IMazeItemProceedInfo>> GetAllProceedInfos { private get; set; }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            if (_Args.Stage == ELevelStage.ReadyToStart && Data.Orientation == MazeOrientation.North)
                MoveMazeItemsGravity(Data.Orientation, Character.Position);
        }

        public void OnShredingerBlockEvent(ShredingerBlockArgs _Args)
        {
            MoveMazeItemsGravity(Data.Orientation, Character.Position);
        }

        public void OnMazeOrientationChanged()
        {
            MoveMazeItemsGravity(Data.Orientation, Character.Position);
        }

        public void OnCharacterMoveStarted(CharacterMovingEventArgs _Args)
        {
            MoveMazeItemsGravity(Data.Orientation, _Args.To);
        }

        #endregion

        #region nonpublic methods
        
        private void MoveMazeItemsGravity(MazeOrientation _Orientation, V2Int _CharacterPoint)
        {
            var dropDirection = RazorMazeUtils.GetDropDirection(_Orientation);
            var infos = ProceedInfos.Where(_Info => _Info.IsProceeding);
            var infosMovedDict = 
                infos.ToDictionary(_Info => _Info, _Info => false);
            TryMoveMazeItemsGravityCore(dropDirection, _CharacterPoint, infosMovedDict);
        }

        private void TryMoveMazeItemsGravityCore(
            V2Int _DropDirection,
            V2Int _CharacterPoint,
            Dictionary<IMazeItemProceedInfo, bool> _InfosMoved)
        {
            var copyOfDict = _InfosMoved.ToList();

            foreach (var kvp in copyOfDict
                .Where(_Kvp => !_Kvp.Value))
            {
                var info = kvp.Key;
                TryMoveBlock(info, _DropDirection, _CharacterPoint, _InfosMoved);
            }
            
            foreach (var info in copyOfDict.Select(_Kvp => _Kvp.Key))
            {
                info.NextPosition = -V2Int.right;
            }
        }

        private bool TryMoveBlock(IMazeItemProceedInfo _Info,
            V2Int _DropDirection,
            V2Int _CharacterPoint,
            Dictionary<IMazeItemProceedInfo, bool> _InfosMoved)
        {
            switch (_Info.Type)
            {
                case EMazeItemType.GravityBlock:
                    return MoveGravityBlock(_Info, _DropDirection, _CharacterPoint, _InfosMoved);
                case EMazeItemType.GravityBlockFree:
                    return MoveGravityBlockFree(_Info, _DropDirection, _CharacterPoint, _InfosMoved);
                case EMazeItemType.GravityTrap:
                    return MoveGravityTrap(_Info, _DropDirection, _CharacterPoint, _InfosMoved);
                default: throw new SwitchCaseNotImplementedException(_Info.Type);
            }
        }

        private bool MoveGravityBlock(
            IMazeItemProceedInfo _Info,
            V2Int _DropDirection,
            V2Int _CharacterPoint,
            Dictionary<IMazeItemProceedInfo, bool> _GravityItemsMovedDict)
        {
            if (!_GravityItemsMovedDict.ContainsKey(_Info))
                return false;
            if (_GravityItemsMovedDict[_Info])
                return true;
            GravityBlockValidPositionDefinitionCycle(
                _Info,
                _DropDirection,
                _CharacterPoint,
                true,
                false,
                out var to,
                out var gravityBlockItemInfo);
            // если для гравитационного блок/ловушка, на который наткнулась ловушка еще определено конечное положение,
            // пытаемся его определить и если получается, двигаем текущий блок
            if (gravityBlockItemInfo != null)
            {
                if (TryMoveBlock(gravityBlockItemInfo, _DropDirection, _CharacterPoint, _GravityItemsMovedDict))
                {
                    GravityBlockValidPositionDefinitionCycle(
                        _Info,
                        _DropDirection,
                        _CharacterPoint,
                        true,
                        true,
                        out to,
                        out gravityBlockItemInfo);
                }
                else
                    return false;
            }
            _Info.ProceedingStage = StageDrop;
            ProceedCoroutine(_Info, MoveMazeItemGravityCoroutine(_Info, to));
            _Info.NextPosition = to;
            _GravityItemsMovedDict[_Info] = true;
            return true;
        }

        // FIXME копипаста из MoveGravityTrap
        private bool MoveGravityBlockFree(
            IMazeItemProceedInfo _Info,
            V2Int _DropDirection,
            V2Int _CharacterPoint,
            Dictionary<IMazeItemProceedInfo, bool> _GravityItemsMovedDict)
        {
            if (!_GravityItemsMovedDict.ContainsKey(_Info))
                return false;
            if (_GravityItemsMovedDict[_Info])
                return true;
            GravityBlockValidPositionDefinitionCycle(
                _Info,
                _DropDirection,
                _CharacterPoint,
                false,
                false,
                out var to,
                out var gravityBlockItemInfo);
            // если для гравитационного блок/ловушка, на который наткнулась ловушка еще определено конечное положение,
            // пытаемся его определить и если получается, двигаем текущий блок
            if (gravityBlockItemInfo != null)
            {
                if (TryMoveBlock(gravityBlockItemInfo, _DropDirection, _CharacterPoint, _GravityItemsMovedDict))
                {
                    GravityBlockValidPositionDefinitionCycle(
                        _Info,
                        _DropDirection,
                        _CharacterPoint,
                        false,
                        true,
                        out to,
                        out gravityBlockItemInfo);
                }
                else
                    return false;
            }
            _Info.ProceedingStage = StageDrop;
            ProceedCoroutine(_Info, MoveMazeItemGravityCoroutine(_Info, to));
            _Info.NextPosition = to;
            _GravityItemsMovedDict[_Info] = true;
            return true;
        }
        
        private bool MoveGravityTrap(
            IMazeItemProceedInfo _Info,
            V2Int _DropDirection,
            V2Int _CharacterPoint,
            Dictionary<IMazeItemProceedInfo, bool> _GravityItemsMovedDict)
        {
            if (!_GravityItemsMovedDict.ContainsKey(_Info))
                return false;
            if (_GravityItemsMovedDict[_Info])
                return true;
            var to = _Info.CurrentPosition;
            IMazeItemProceedInfo gravityBlockItemInfo;
            var infos = GetAllProceedInfos().ToList();
            while (IsValidPositionForMove(to + _DropDirection, infos, false, out gravityBlockItemInfo))
                to += _DropDirection;
            // если для гравитационного блок/ловушка, на который наткнулась ловушка еще определено конечное положение,
            // пытаемся его определить и если получается, двигаем текущий блок
            if (gravityBlockItemInfo != null)
            {
                if (TryMoveBlock(gravityBlockItemInfo, _DropDirection, _CharacterPoint, _GravityItemsMovedDict))
                {
                    while (IsValidPositionForMove(to + _DropDirection, infos, true, out gravityBlockItemInfo))
                        to += _DropDirection;
                }
                else
                    return false;
            }
            _Info.ProceedingStage = StageDrop;
            ProceedCoroutine(_Info, MoveMazeItemGravityCoroutine(_Info, to));
            _Info.NextPosition = to;
            _GravityItemsMovedDict[_Info] = true;
            return true;
        }
        
        private void GravityBlockValidPositionDefinitionCycle(
            IMazeItemProceedInfo _Info,
            V2Int _DropDirection,
            V2Int? _CharacterPoint,
            bool _CheckMazeItemPath,
            bool _CheckNextPos,
            out V2Int _To,
            out IMazeItemProceedInfo _GravityBlockItemInfo)
        {
            bool doMove = false;
            var pos = _Info.CurrentPosition;
            var path = _Info.Path;
            int currPathIdx = path.IndexOf(_Info.CurrentPosition);
            V2Int? altPos = null;
            var infos = GetAllProceedInfos().ToList();
            while (IsValidPositionForMove(pos + _DropDirection, infos, _CheckNextPos, out _GravityBlockItemInfo))
            {
                pos += _DropDirection;
                // если новая позиция блока совпадает с позицией персонажа, записываем ее в отдельную переменную
                if (pos == _CharacterPoint)
                {
                    altPos = pos - _DropDirection;
                    break;
                }
                if (!_CheckMazeItemPath)
                    continue;
                // если новая позиция блока не находится на узле пути, проверяем следующую позицию
                if (_Info.Path.All(_Pos1 => pos != _Pos1))
                    continue;
                // если текущая позиция блока не находится на узле пути, а новая - находится, то движение разрешено  
                
                if (currPathIdx == -1)
                {
                    doMove = true;
                    break;
                }
                // если текущая позиция блока находится на узле пути, и новая тоде находится на узле пути,
                // но они не являются близжайшими, проверяем следующую позицию
                if (Math.Abs(path.IndexOf(pos) - path.IndexOf(_Info.CurrentPosition)) > 1)
                    continue;
                doMove = true;
                break;
            }
            // если текущая позиция находится на одном из близжайших участков пути, разрешаем движение
            if (currPathIdx != -1 && _CheckMazeItemPath)
            {
                if (currPathIdx - 1 >= 0 && PathContainsItem(path[currPathIdx - 1], path[currPathIdx], pos)
                || currPathIdx + 1 < path.Count && PathContainsItem(path[currPathIdx], path[currPathIdx + 1], pos))
                    doMove = true;
            }
            else
                doMove = true;
            
            if (altPos.HasValue)
                pos = altPos.Value;
            _To = doMove ? pos : _Info.CurrentPosition;
        }

        private IEnumerator MoveMazeItemGravityCoroutine(
            IMazeItemProceedInfo _Info,
            V2Int _To)
        {
            var from = _Info.CurrentPosition;
            if (from == _To)
            {
                _Info.ProceedingStage = StageIdle;
                yield break;
            }
            float speed = _Info.Type == EMazeItemType.GravityBlock || _Info.Type == EMazeItemType.GravityBlockFree
                ? Settings.GravityBlockSpeed
                : Settings.GravityTrapSpeed;
            var busyPositions = _Info.BusyPositions;
            InvokeMoveStarted(new MazeItemMoveEventArgs(
                _Info, from, _To, speed, 0, busyPositions));
            var direction = (_To - from).Normalized;
            float distance = V2Int.Distance(from, _To);
            yield return Coroutines.Lerp(
                0f,
                1f,
                distance / speed,
                _Progress =>
                {
                    var addict = direction * (_Progress + 0.1f) * distance;
                    busyPositions.Clear();
                    busyPositions.Add(from + V2Int.Floor(addict));
                    if (busyPositions[0] != _To)
                        busyPositions.Add(from + V2Int.Ceil(addict));
                    InvokeMoveContinued(new MazeItemMoveEventArgs(
                        _Info, from, _To, speed, _Progress, busyPositions));
                },
                GameTicker,
                (_Stopped, _Progress) =>
                {
                    var to = !_Stopped ? _To : _Info.BusyPositions[0];  
                    _Info.CurrentPosition = to;
                    InvokeMoveFinished(new MazeItemMoveEventArgs(
                        _Info, from, to, speed, _Progress, busyPositions, _Stopped));
                    _Info.ProceedingStage = StageIdle;
                    busyPositions.Clear();
                    busyPositions.Add(to);
                });
        }

        private bool IsValidPositionForMove(
            V2Int _Position,
            IEnumerable<IMazeItemProceedInfo> _Infos,
            bool _CheckNextPos,
            out IMazeItemProceedInfo _GravityBlockItemInfo)
        {
            bool isOnNode = PathItemsProceeder.PathProceeds.Keys.Any(_Pos => _Pos == _Position);
            var staticBlockItems = GetStaticBlockItems(GetAllProceedInfos());
            bool isOnStaticBlockItem = staticBlockItems.Any(_N =>
            {
                if (_N.CurrentPosition != _Position)
                    return false;
                if (_N.Type == EMazeItemType.ShredingerBlock
                    && _N.ProceedingStage == ShredingerBlocksProceeder.StageClosed)
                    return true;
                return _N.Type != EMazeItemType.ShredingerBlock;
            });
            _GravityBlockItemInfo = _Infos.FirstOrDefault(_Inf => 
                RazorMazeUtils.GravityItemTypes().Contains(_Inf.Type) 
                && _Position == (_CheckNextPos ? _Inf.NextPosition : _Inf.CurrentPosition));
            bool result = isOnNode && !isOnStaticBlockItem && _GravityBlockItemInfo == null;
            return result;
        }
        
        private static IEnumerable<IMazeItemProceedInfo> GetStaticBlockItems(IEnumerable<IMazeItemProceedInfo> _Items)
        {
            return _Items.Where(_Item =>
                _Item.Type == EMazeItemType.Block
                || _Item.Type == EMazeItemType.Turret
                || _Item.Type == EMazeItemType.TrapReact
                || _Item.Type == EMazeItemType.TrapIncreasing
                || _Item.Type == EMazeItemType.ShredingerBlock
                || _Item.Type == EMazeItemType.Portal);
        }

        #endregion
    }
}