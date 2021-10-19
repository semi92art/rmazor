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
                const int triesCount = 10;
                for (int i = 0; i < triesCount; i++)
                    MoveMazeItemsGravityCore(dropDirection, _CharacterPoint, infosMovedDict, false);
                MoveMazeItemsGravityCore(dropDirection, _CharacterPoint, infosMovedDict, true);
            }
            else
            {
                foreach (var info in infos)
                    TryMoveBlock(info, dropDirection, _CharacterPoint, true);
            }
        }

        private void MoveMazeItemsGravityCore(
            V2Int _DropDirection,
            V2Int _CharacterPoint,
            Dictionary<IMazeItemProceedInfo, bool> _InfosMoved, 
            bool _Forced)
        {
            var copyOfDict = _InfosMoved.ToList();
            foreach (var kvp in copyOfDict
                .Where(_Kvp => !_Kvp.Value))
            {
                var info = kvp.Key;
                _InfosMoved[info] = TryMoveBlock(info, _DropDirection, _CharacterPoint, _Forced);
            }
            copyOfDict = _InfosMoved.Reverse().ToList();
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
            V2Int? posNearCharacter = null;
            bool isOnGravityBlockItem;
            while (IsValidPositionForMove(pos + _DropDirection, gravityProceedInfos, out isOnGravityBlockItem))
            {
                pos += _DropDirection;
                // если новая позиция блока совпадает с позицией персонажа, записываем ее в отдельную переменную
                if (_CharacterPoint == pos)
                    posNearCharacter = pos - _DropDirection;
                // если новая позиция блока не находится на узле пути, проверяем следующую позицию
                if (_Info.Path.All(_Pos => pos != _Pos))
                    continue;
                // если текущая позиция блока не находится на узле пути, а новая - находится, то движение разрешено  
                int currentPathIndex = _Info.Path.IndexOf(_Info.CurrentPosition);
                if (currentPathIndex == -1)
                {
                    doMove = true;
                    break;
                }
                // если текущая позиция блока находится на узле пути, и новая тоде находится на узле пути,
                // но они не являются близжайшими, проверяем следующую позицию
                if (Math.Abs(_Info.Path.IndexOf(pos) - _Info.Path.IndexOf(_Info.CurrentPosition)) > 1)
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
            var to = posNearCharacter ?? pos;
            if (_Info.CurrentPosition == to)
                return false;
            _Info.ProceedingStage = StageDrop;
            _Info.NextPosition = to;
            ProceedCoroutine(MoveMazeItemGravityCoroutineCore(_Info, to));
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
            var to = _Info.CurrentPosition;
            bool isOnGravityBlockItem;
            while (IsValidPositionForMove(to + _DropDirection, gravityProceedInfos, out isOnGravityBlockItem))
                to += _DropDirection;
            if (isOnGravityBlockItem && !_Forced || _Info.CurrentPosition == to)
                return false;
            _Info.ProceedingStage = StageDrop;
            _Info.NextPosition = to;
            ProceedCoroutine(MoveMazeItemGravityCoroutineCore(_Info, to));
            return true;
        }

        private IEnumerator MoveMazeItemGravityCoroutineCore(
            IMazeItemProceedInfo _Info,
            V2Int _To)
        {
            var from = _Info.CurrentPosition;
            float speed = _Info.Type == EMazeItemType.GravityBlock
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
                || _Item.Type == EMazeItemType.ShredingerBlock
                || _Item.Type == EMazeItemType.Portal);
        }

        #endregion
    }
}