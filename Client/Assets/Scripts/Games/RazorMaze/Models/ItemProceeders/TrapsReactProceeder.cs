using System;
using System.Collections;
using System.Linq;
using Games.RazorMaze.Models.ProceedInfos;
using TimeProviders;
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
    
    public interface ITrapsReactProceeder : IItemsProceeder, ICharacterMoveContinued
    {
        event MazeItemTrapReactEventHandler TrapReactStageChanged;
    }
    
    public class TrapsReactProceeder : ItemsProceederBase, ITrapsReactProceeder
    {
        #region constants
        
        public const int StagePreReact = 1;
        public const int StageReact = 2;
        public const int StageAfterReact = 3;
        
        #endregion
        
        #region nonpublic members
        
        // private V2Int m_CharacterPosCheck;
        protected override EMazeItemType[] Types => new[] {EMazeItemType.TrapReact};

        #endregion

        #region inject
        
        public TrapsReactProceeder(ModelSettings _Settings, IModelMazeData _Data, IModelCharacter _Character) 
            : base(_Settings, _Data, _Character) { }

        #endregion
        
        #region api
        
        public event MazeItemTrapReactEventHandler TrapReactStageChanged;

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            // FIXME возможно лишнее
            // if (!Data.ProceedingMazeItems)
            //     return;
            // var addictRaw = (_Args.To.ToVector2() - _Args.From.ToVector2()) * _Args.Progress;
            // var addict = new V2Int(addictRaw);
            // var newPos = _Args.From + addict;
            // if (m_CharacterPosCheck == newPos)
            //     return;
            // m_CharacterPosCheck = newPos;
            ProceedTraps();
        }

        #endregion
        
        #region nonpublic methods

        private void ProceedTraps()
        {
            var infos = GetProceedInfos(Types);
            foreach (var info in infos.Values.Where(_Info => _Info.IsProceeding && _Info.ReadyToSwitchStage))
            {
                if (info.Item.Position + info.Item.Direction != Character.Position)
                    continue;
                Coroutines.Run(ProceedTrap(info));
            }
            //
            // foreach (var kvp in infos
            //     .Where(_Kvp => 
            //         _Kvp.Value.IsProceeding && _Kvp.Value.ProceedingStage == StageReact))
            // {
            //     CheckForCharacterDeath(kvp.Value, kvp.Key.Position, kvp.Key.Direction);
            // }
        }

        private IEnumerator ProceedTrap(IMazeItemProceedInfo _Info)
        {
            _Info.ReadyToSwitchStage = false;
            _Info.ProceedingStage++;
            float duration = GetStageDuration(_Info.ProceedingStage); 
            TrapReactStageChanged?.Invoke(
                new MazeItemTrapReactEventArgs(_Info.Item, _Info.ProceedingStage, duration));
            float time = GameTimeProvider.Instance.Time;
            yield return Coroutines.WaitWhile(
                () => time + duration > GameTimeProvider.Instance.Time,
                () =>
                {
                    if (_Info.ProceedingStage == StageAfterReact)
                    {
                        _Info.ProceedingStage = StageIdle;
                        _Info.ReadyToSwitchStage = true;
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

        // private void CheckForCharacterDeath(IMazeItemProceedInfo _Info, V2Int _Position, V2Int _Direction)
        // {
        //     if (Character.Position != _Position + _Direction) 
        //         return;
        //     KillerProceedInfo = _Info;
        //     Character.RaiseDeath();
        // }
        
        #endregion
    }
}