using System.Collections;
using System.Linq;
using Common;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;

namespace RMAZOR.Models.ItemProceeders
{
    public class SpearEventArgs : MazeItemProceedInfoEventArgs
    {
        public SpearEventArgs(IMazeItemProceedInfo _Info) : base(_Info) { }
    }

    public delegate void SpearEventHandler(SpearEventArgs _Args);

    public interface ISpearsProceeder : IItemsProceeder, ICharacterMoveFinished
    {
        event SpearEventHandler SpearAppear;
        event SpearEventHandler SpearShot;
    }
    
    public class SpearsProceeder : ItemsProceederBase, ISpearsProceeder
    {
        #region nonpublic members

        protected override EMazeItemType[] Types => new[] {EMazeItemType.Spear};

        #endregion

        #region inject
        
        private SpearsProceeder(
            ModelSettings      _Settings,
            IModelData         _Data,
            IModelCharacter    _Character,
            IModelGameTicker   _GameTicker,
            IModelMazeRotation _Rotation)
            : base(
                _Settings,
                _Data,
                _Character,
                _GameTicker, 
                _Rotation) { }

        #endregion

        #region api

        public event SpearEventHandler SpearAppear;
        public event SpearEventHandler SpearShot;

        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            if (_Args.BlockOnFinish == null
                || !Types.Contains(_Args.BlockOnFinish.Type)
                || _Args.BlockOnFinish.ProceedingStage != ModelCommonData.StageIdle)
            {
                return;
            }
            TriggerSpear(_Args.BlockOnFinish);
        }
        
        #endregion

        #region nonpublic methods

        private void TriggerSpear(IMazeItemProceedInfo _Info)
        {
            var args = new SpearEventArgs(_Info);
            SpearAppear?.Invoke(args);
            _Info.ProceedingStage = ModelCommonData.SpearStageAppear;
            ProceedCoroutine(_Info, SpearAppearCoroutine(_Info));
        }

        private IEnumerator SpearAppearCoroutine(IMazeItemProceedInfo _Info)
        {
            float time = GameTicker.Time;
            while (time + Settings.spearAppearPause > GameTicker.Time)
                yield return null;
            _Info.ProceedingStage = ModelCommonData.SpearStageShoot;
            yield return SpearShotCoroutine(_Info);
        }

        private IEnumerator SpearShotCoroutine(IMazeItemProceedInfo _Info)
        {
            Dbg.Log(nameof(SpearShotCoroutine));
            int shot = 0;
            while (shot++ < Settings.spearShotsPerUnit)
                yield return Cor.Delay(
                    Settings.spearShotPause,
                    GameTicker,
                    () => SpearShot?.Invoke(new SpearEventArgs(_Info)));
        }

        #endregion
    }
}