using System.Collections;
using Common.Entities;
using Common.Exceptions;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;
using UnityEngine.Events;

namespace RMAZOR.Models.ItemProceeders
{

    public interface ITrapsMovingProceeder : IMovingItemsProceeder { }
    

    public class TrapsMovingProceeder : MovingItemsProceederBase, IUpdateTick, ITrapsMovingProceeder
    {
        #region constants

        public const int StageMoving = 1;

        #endregion

        #region inject

        public TrapsMovingProceeder(
            ModelSettings _Settings, 
            IModelData _Data,
            IModelCharacter _Character,
            IModelGameTicker _GameTicker)
            : base(_Settings, _Data, _Character, _GameTicker) { }
        
        #endregion
        
        #region api

        protected override EMazeItemType[] Types => new[] {EMazeItemType.TrapMoving};

        public void UpdateTick()
        {
            ProceedTrapsMoving();
        }
        
        #endregion

        #region nonpublic methods
        
        private void ProceedTrapsMoving()
        {
            for (int i = 0; i < ProceedInfos.Length; i++)
            {
                var info = ProceedInfos[i];
                if (!info.IsProceeding)
                    continue;
                if (info.ProceedingStage != StageIdle)
                    continue;
                info.PauseTimer += GameTicker.DeltaTime;
                if (info.PauseTimer < Settings.MovingItemsPause)
                    continue;
                info.PauseTimer = 0;
                info.ProceedingStage = StageMoving;
                ProceedTrapMoving(info, () => info.ProceedingStage = StageIdle);
            }
        }
        
        private void ProceedTrapMoving(
            IMazeItemProceedInfo _Info, 
            UnityAction _OnFinish)
        {
            V2Int from = _Info.CurrentPosition;
            V2Int to;
            int idx = _Info.Path.IndexOf(_Info.CurrentPosition);
            var path = _Info.Path;
            switch (_Info.MoveByPathDirection)
            {
                case EMoveByPathDirection.Forward:
                    if (idx == path.Count - 1)
                    {
                        idx--;
                        _Info.MoveByPathDirection = EMoveByPathDirection.Backward;
                    }
                    else
                        idx++;
                    to = path[idx];
                    break;
                case EMoveByPathDirection.Backward:
                    if (idx == 0)
                    {
                        idx++;
                        _Info.MoveByPathDirection = EMoveByPathDirection.Forward;
                    }
                    else
                        idx--;
                    to = path[idx];
                    break;
                default: throw new SwitchCaseNotImplementedException(_Info.MoveByPathDirection);
            }
            var coroutine = MoveTrapMovingCoroutine(_Info, from, to, _OnFinish);
            ProceedCoroutine(_Info, coroutine);
        }
        
        private IEnumerator MoveTrapMovingCoroutine(
            IMazeItemProceedInfo _Info, 
            V2Int _From,
            V2Int _To,
            UnityAction _OnFinish)
        {
            _Info.IsMoving = true;
            _Info.CurrentPosition = _From;
            InvokeMoveStarted(new MazeItemMoveEventArgs(
                _Info, _From, _To, Settings.MovingItemsSpeed,0));
            float distance = V2Int.Distance(_From, _To);
            yield return Cor.Lerp(
                0f,
                1f,
                distance / Settings.MovingItemsSpeed,
                _Progress =>
                {
                    var precisePosition = V2Int.Lerp(_From, _To, _Progress);
                    _Info.CurrentPosition = V2Int.Round(precisePosition);
                    InvokeMoveContinued(new MazeItemMoveEventArgs(
                        _Info, _From, _To, Settings.MovingItemsSpeed, _Progress));
                },
                GameTicker,
                (_Stopped, _Progress) =>
                {
                    _Info.CurrentPosition = _To;
                    _Info.IsMoving = false;
                    _OnFinish?.Invoke();
                    InvokeMoveFinished(new MazeItemMoveEventArgs(
                        _Info, _From, _To, Settings.MovingItemsSpeed,1f));
                });
        }
        
        #endregion
    }
}