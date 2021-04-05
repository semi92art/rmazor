using System;
using System.Collections;
using System.Linq;
using Entities;
using Games.RazorMaze.Models.ProceedInfos;
using Utils;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public class MazeItemTrapReactEventArgs : EventArgs
    {
        public MazeItem Item { get; }
        public int Stage { get; }
        public float Duration { get; }

        public MazeItemTrapReactEventArgs(MazeItem _Item, int _Stage, float _Duration)
        {
            Item = _Item;
            Stage = _Stage;
            Duration = _Duration;
        }
    }

    public delegate void MazeItemTrapReactEventHandler(MazeItemTrapReactEventArgs _Args);
    
    public interface ITrapsReactProceeder : IOnMazeChanged, ICharacterMoveContinued
    {
        event MazeItemTrapReactEventHandler TrapReactStageChanged;
    }
    
    public class TrapsReactProceeder : ItemsProceederBase, ITrapsReactProceeder
    {
        #region constants
        
        public const int StageIdle = 0;
        public const int StagePreReact = 1;
        public const int StageReact = 2;
        public const int StageAfterReact = 3;
        
        #endregion
        
        #region nonpublic members
        
        private V2Int m_CharacterPosCheck;
        protected override EMazeItemType[] Types => new[] {EMazeItemType.TrapReact};

        #endregion

        #region inject
        
        public TrapsReactProceeder(ModelSettings _Settings, IModelMazeData _Data) 
            : base(_Settings, _Data) { }

        #endregion
        
        #region api
        
        public event MazeItemTrapReactEventHandler TrapReactStageChanged;

        public void OnMazeChanged(MazeInfo _Info)
        {
            CollectItems(_Info);
        }

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            if (!Data.ProceedingMazeItems)
                return;
            var addictRaw = (_Args.To.ToVector2() - _Args.From.ToVector2()) * _Args.Progress;
            var addict = new V2Int(addictRaw);
            var newPos = _Args.From + addict;
            if (m_CharacterPosCheck == newPos)
                return;
            m_CharacterPosCheck = newPos;
            ProceedTraps();
        }

        #endregion
        
        #region nonpublic methods

        private void ProceedTraps()
        {
            foreach (var type in Types)
            {
                var infos = GetProceedInfos(type);
                foreach (var info in infos.Values.Where(_Info => !_Info.IsProceeding))
                {
                    var item = info.Item;
                    var trapExtractPos = item.Position + item.Direction;
                    if (trapExtractPos != m_CharacterPosCheck)
                        continue;
                    info.IsProceeding = true;
                    Coroutines.Run(ProceedTrap(info));
                }
            }
        }

        private IEnumerator ProceedTrap(IMazeItemProceedInfo _Info)
        {
            _Info.IsProceeding = true;
            _Info.ProceedingStage++;
            float duration = GetStageDuration(_Info.ProceedingStage); 
            TrapReactStageChanged?.Invoke(new MazeItemTrapReactEventArgs(_Info.Item, _Info.ProceedingStage, duration));
            float time = GameTimeProvider.Instance.Time;
            yield return Coroutines.WaitWhile(
                () => time + duration > GameTimeProvider.Instance.Time,
                () =>
                {
                    if (_Info.ProceedingStage == StageAfterReact)
                    {
                        _Info.ProceedingStage = StageIdle;
                        _Info.IsProceeding = false;
                        return;
                    }
                    Coroutines.Run(ProceedTrap(_Info));
                });
        }

        private float GetStageDuration(int _Stage)
        {
            switch (_Stage)
            {
                case StagePreReact:
                    return Settings.trapPreReactTime;
                case StageReact:
                    return Settings.trapReactTime;
                case StageAfterReact:
                    return Settings.trapAfterReactTime;
                default: return 0;
            }
        }
        
        #endregion
    }
}