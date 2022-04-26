using System;
using System.Collections;
using System.Linq;
using Common;
using Common.Entities;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;

namespace RMAZOR.Models.ItemProceeders
{
    public class BazookaEventArgs : MazeItemProceedInfoEventArgs
    {
        public BazookaEventArgs(IMazeItemProceedInfo _Info) : base(_Info) { }
    }

    public delegate void BazookaEventHandler(BazookaEventArgs _Args);

    public interface IBazookasProceeder : IItemsProceeder, IGetAllProceedInfos, ICharacterMoveFinished
    {
        event BazookaEventHandler BazookaAppear;
        event BazookaEventHandler BazookaShot;
    }
    
    public class BazookasProceeder : ItemsProceederBase, IBazookasProceeder
    {
        #region nonpublic members
        
        protected override EMazeItemType[] Types => new[] {EMazeItemType.Bazooka};

        #endregion

        #region inject
        
        public BazookasProceeder(
            ModelSettings    _Settings,
            IModelData       _Data,
            IModelCharacter  _Character,
            IModelGameTicker _GameTicker)
            : base(_Settings, _Data, _Character, _GameTicker) { }

        #endregion

        #region api

        public Func<IMazeItemProceedInfo[]> GetAllProceedInfos { get; set; }
        
        public event BazookaEventHandler BazookaAppear;
        public event BazookaEventHandler BazookaShot;

        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            if (_Args.BlockOnFinish == null
                || !Types.Contains(_Args.BlockOnFinish.Type)
                || _Args.BlockOnFinish.ProceedingStage != ModelCommonData.StageIdle)
            {
                return;
            }
            TriggerBazooka(_Args.BlockOnFinish);
        }
        
        #endregion

        #region nonpublic methods

        private void TriggerBazooka(IMazeItemProceedInfo _Info)
        {
            var args = new BazookaEventArgs(_Info);
            BazookaAppear?.Invoke(args);
            _Info.ProceedingStage = ModelCommonData.BazookaStageAppear;
            ProceedCoroutine(_Info, BazookaAppearCoroutine(_Info));
        }

        private IEnumerator BazookaAppearCoroutine(IMazeItemProceedInfo _Info)
        {
            float time = GameTicker.Time;
            while (time + Settings.bazookaAppearPause > GameTicker.Time)
                yield return null;
            _Info.ProceedingStage = ModelCommonData.BazookaStageShoot;
            yield return BazookaShotCoroutine(_Info);
        }

        private IEnumerator BazookaShotCoroutine(IMazeItemProceedInfo _Info)
        {
            Dbg.Log(nameof(BazookaShotCoroutine));
            int shot = 0;
            while (shot++ < Settings.bazookaShotsPerUnit)
                yield return Cor.Delay(
                    Settings.bazookaShotPause,
                    () => BazookaShot?.Invoke(new BazookaEventArgs(_Info)));
        }

        #endregion
    }
}