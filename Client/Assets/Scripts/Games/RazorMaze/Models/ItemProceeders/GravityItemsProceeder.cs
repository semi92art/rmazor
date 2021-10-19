using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Entities;
using Games.RazorMaze.Models.ProceedInfos;
using Ticker;
using Utils;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public interface IGravityItemsProceeder : IMovingItemsProceeder, ICharacterMoveStarted
    {
        void OnMazeOrientationChanged();
    }
    
    public class GravityItemsProceeder : MovingItemsProceederBase, IGravityItemsProceeder, IGetAllProceedInfos
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
            IGameTicker _GameTicker,
            IPathItemsProceeder _PathItemsProceeder) 
            : base (_Settings, _Data, _Character, _LevelStaging, _GameTicker)
        {
            PathItemsProceeder = _PathItemsProceeder;
        }
        
        #endregion
        
        #region api
        
        public override EMazeItemType[] Types => new[] {EMazeItemType.GravityBlock, EMazeItemType.GravityTrap};

        public Func<IEnumerable<IMazeItemProceedInfo>> GetAllProceedInfos { private get; set; }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            if (_Args.Stage == ELevelStage.ReadyToStartOrContinue && Data.Orientation == MazeOrientation.North)
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
            var infos = GetProceedInfos(Types).Where(_Info => _Info.IsProceeding).ToList();

            if (infos.Any(_Info => _Info.Type == EMazeItemType.GravityTrap))
            {
                var infosMovedDict = infos.ToDictionary(_Info => _Info, _Info => false);
            
                for (int i = 0; i < 10; i++)
                {
                    MoveMazeItemsGravityCore(dropDirection, _CharacterPoint, infosMovedDict, false, false);
                    MoveMazeItemsGravityCore(dropDirection, _CharacterPoint, infosMovedDict, true, false);
                }
                MoveMazeItemsGravityCore(dropDirection, _CharacterPoint, infosMovedDict, false, true);
                MoveMazeItemsGravityCore(dropDirection, _CharacterPoint, infosMovedDict, true, true);
            }
            else
            {
                foreach (var info in infos)
                    TryMoveBlock(info, dropDirection, _CharacterPoint, false);
                foreach (var info in infos)
                    TryMoveBlock(info, dropDirection, _CharacterPoint, true);
            }
        }

        private void MoveMazeItemsGravityCore(
            V2Int _DropDirection,
            V2Int _CharacterPoint,
            Dictionary<IMazeItemProceedInfo, bool> _InfosMoved, 
            bool _Reverse,
            bool _Forced)
        {
            var copyOfDict = _Reverse ? _InfosMoved.Reverse().ToList() : _InfosMoved.ToList();
            foreach (var kvp in copyOfDict
                .Where(_Kvp => !_Kvp.Value))
            {
                var info = kvp.Key;
                _InfosMoved[info] = TryMoveBlock(info, _DropDirection, _CharacterPoint, _Forced);
            }
        }

        private bool TryMoveBlock(IMazeItemProceedInfo _Info,
            V2Int _DropDirection,
            V2Int _CharacterPoint,
            bool _Forced)
        {
            if (_Info.Type == EMazeItemType.GravityBlock)
                return MoveGravityBlock(_Info, _DropDirection, _CharacterPoint, _Forced);
            if (_Info.Type == EMazeItemType.GravityTrap)
                return MoveGravityTrap(_Info, _DropDirection, _Forced);
            return false;
        }

        private bool MoveGravityBlock(
            IMazeItemProceedInfo _Info,
            V2Int _DropDirection,
            V2Int _CharacterPoint,
            bool _Forced)
        {
            var gravityProceedInfos = ProceedInfos
                .SelectMany(_Infos => _Infos.Value)
                .ToList();
            var pos = _Info.CurrentPosition;
            bool doMove = false;
            V2Int? altPos = null;
            bool isOnGravityBlockItem;
            while (IsValidPositionForMove(pos + _DropDirection, gravityProceedInfos, out isOnGravityBlockItem))
            {
                pos += _DropDirection;
                if (_CharacterPoint == pos)
                    altPos = pos - _DropDirection;
                var pos1 = pos;
                if (_Info.Path.All(_Pos => pos1 != _Pos))
                    continue;
                int fromPosIdx = _Info.Path.IndexOf(_Info.CurrentPosition);
                if (fromPosIdx == -1)
                {
                    doMove = true;
                    break;
                }
                int toPosIds = _Info.Path.IndexOf(pos);
                if (Math.Abs(toPosIds - fromPosIdx) > 1)
                    continue;
                doMove = true;
                break;
            }
            if (isOnGravityBlockItem && !_Forced)
                return false;
            if (!doMove || pos == _Info.CurrentPosition)
            {
                _Info.ProceedingStage = StageIdle;                       
                return false;
            }
            var from = _Info.CurrentPosition;
            var to = altPos ?? pos;
            if (from == to)
                return false;
            _Info.ProceedingStage = StageDrop;
            _Info.NextPosition = to;
            ProceedCoroutine(MoveMazeItemGravityCoroutineCore(_Info, from, to));
            return true;
        }

        private bool MoveGravityTrap(
            IMazeItemProceedInfo _Info,
            V2Int _DropDirection,
            bool _Forced)
        {
            var gravityProceedInfos = ProceedInfos
                .SelectMany(_Infos => _Infos.Value)
                .ToList();
            var pos = _Info.CurrentPosition;
            bool isOnGravityBlockItem;
            while (IsValidPositionForMove(pos + _DropDirection, gravityProceedInfos, out isOnGravityBlockItem))
                pos += _DropDirection;
            if (isOnGravityBlockItem && !_Forced)
                return false;
            var from = _Info.CurrentPosition;
            var to = pos;
            if (from == to)
                return false;
            _Info.ProceedingStage = StageDrop;
            _Info.NextPosition = to;
            ProceedCoroutine(MoveMazeItemGravityCoroutineCore(_Info, from, to));
            return true;
        }

        private IEnumerator MoveMazeItemGravityCoroutineCore(
            IMazeItemProceedInfo _Info,
            V2Int _From,
            V2Int _To)
        {
            float speed = _Info.Type == EMazeItemType.GravityBlock
                ? Settings.GravityBlockSpeed
                : Settings.GravityTrapSpeed;
            var busyPositions = _Info.BusyPositions;
            InvokeMoveStarted(new MazeItemMoveEventArgs(
                _Info, _From, _To, speed, 0, busyPositions));
            var direction = (_To - _From).Normalized;
            float distance = V2Int.Distance(_From, _To);
            yield return Coroutines.Lerp(
                0f,
                1f,
                distance / speed,
                _Progress =>
                {
                    var addict = direction * (_Progress + 0.1f) * distance;
                    busyPositions.Clear();
                    busyPositions.Add(_From + V2Int.Floor(addict));
                    if (busyPositions[0] != _To)
                        busyPositions.Add(_From + V2Int.Ceil(addict));
                    InvokeMoveContinued(new MazeItemMoveEventArgs(
                        _Info, _From, _To, speed, _Progress, busyPositions));
                },
                GameTicker,
                (_Stopped, _Progress) =>
                {
                    var to = !_Stopped ? _To : _Info.BusyPositions[0];  
                    _Info.CurrentPosition = to;
                    InvokeMoveFinished(new MazeItemMoveEventArgs(
                        _Info, _From, to, speed, _Progress, busyPositions, _Stopped));
                    _Info.ProceedingStage = StageIdle;
                    busyPositions.Clear();
                    busyPositions.Add(to);
                });
        }

        private bool IsValidPositionForMove(
            V2Int _Position,
            IEnumerable<IMazeItemProceedInfo> _Infos,
            out bool _IsOnGravityBlockItem)
        {
            bool isOnNode = PathItemsProceeder.PathProceeds.Keys.Any(_Pos => _Pos == _Position);
            var staticBlockItems = GetStaticBlockItems(GetAllProceedInfos());
            bool isOnStaticBlockItem = staticBlockItems.Any(_N => _N.CurrentPosition == _Position);
            _IsOnGravityBlockItem = _Infos.Any(_Inf => _Inf.NextPosition == _Position);
            return isOnNode && !isOnStaticBlockItem && !_IsOnGravityBlockItem;
        }
        
        private static IEnumerable<IMazeItemProceedInfo> GetStaticBlockItems(IEnumerable<IMazeItemProceedInfo> _Items)
        {
            return _Items.Where(_Item =>
                _Item.Type == EMazeItemType.Block
                || _Item.Type == EMazeItemType.Turret
                || _Item.Type == EMazeItemType.TrapReact
                || _Item.Type == EMazeItemType.TrapIncreasing
                || _Item.Type == EMazeItemType.ShredingerBlock);
        }

        #endregion
    }
}