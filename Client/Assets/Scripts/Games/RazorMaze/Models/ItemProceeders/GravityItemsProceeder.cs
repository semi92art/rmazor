using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            IGameTicker _GameTicker,
            IPathItemsProceeder _PathItemsProceeder) 
            : base (_Settings, _Data, _Character, _GameTicker)
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
            if (_Args.Stage == ELevelStage.StartedOrContinued)
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
            foreach (var info in GetProceedInfos(Types).Where(_Info => _Info.IsProceeding))
            {
                IEnumerator coroutine = null;
                if (info.Type == EMazeItemType.GravityBlock)
                    coroutine = MoveGravityBlockCoroutine(info, dropDirection, _CharacterPoint);
                else if (info.Type == EMazeItemType.GravityTrap)
                    coroutine = MoveGravityTrapCoroutine(info, dropDirection);
                ProceedCoroutine(coroutine);
            }
        }

        private IEnumerator MoveGravityBlockCoroutine(
            IMazeItemProceedInfo _Info,
            V2Int _DropDirection,
            V2Int _CharacterPoint)
        {
            var gravityProceedInfos = ProceedInfos
                .SelectMany(_Infos => _Infos.Value)
                .ToList();
            yield return Coroutines.WaitWhile(
                () => _Info.ProceedingStage == StageDrop,
                () =>
                {
                    var pos = _Info.CurrentPosition;
                    bool doMove = false;
                    V2Int? altPos = null;
                    while (IsValidPositionForMove(pos + _DropDirection, gravityProceedInfos))
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
                    if (!doMove || pos == _Info.CurrentPosition)
                    {
                        _Info.ProceedingStage = StageIdle;                       
                        return;
                    }
                    var from = _Info.CurrentPosition;
                    var to = altPos ?? pos;

                    if (from == to)
                        return;
                    _Info.ProceedingStage = StageDrop;
                    Coroutines.Run(MoveMazeItemGravityCoroutineCore(_Info, from, to));
                });
        }

        private IEnumerator MoveGravityTrapCoroutine(
            IMazeItemProceedInfo _Info,
            V2Int _DropDirection)
        {
            var gravityProceedInfos = ProceedInfos
                .SelectMany(_Infos => _Infos.Value)
                .ToList();
            yield return Coroutines.WaitWhile(
                () => _Info.ProceedingStage == StageDrop,
                () =>
                {
                    var pos = _Info.CurrentPosition;
                    while (IsValidPositionForMove(pos + _DropDirection, gravityProceedInfos))
                        pos += _DropDirection;
                    var from = _Info.CurrentPosition;
                    var to = pos;
                    if (from == to)
                        return;
                    _Info.ProceedingStage = StageDrop;
                    Coroutines.Run(MoveMazeItemGravityCoroutineCore(_Info, from, to));
                });
        }

        private IEnumerator MoveMazeItemGravityCoroutineCore(
            IMazeItemProceedInfo _Info,
            V2Int _From,
            V2Int _To)
        {
            float speed = _Info.Type == EMazeItemType.GravityBlock
                ? Settings.gravityBlockSpeed
                : Settings.gravityTrapSpeed;
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
            IEnumerable<IMazeItemProceedInfo> _Infos)
        {
            bool isOnNode = PathItemsProceeder.PathProceeds.Keys.Any(_Pos => _Pos == _Position);
            var staticBlockItems = GetStaticBlockItems(GetAllProceedInfos());
            bool isOnStaticBlockItem = staticBlockItems.Any(_N => _N.CurrentPosition == _Position);
            bool isOnMovingBlockItem = _Infos.Any(_Inf => _Inf.CurrentPosition == _Position);
            return isOnNode && !isOnStaticBlockItem && !isOnMovingBlockItem;
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