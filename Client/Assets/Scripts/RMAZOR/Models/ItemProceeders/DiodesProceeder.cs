using System;
using System.Linq;
using mazing.common.Runtime.Ticker;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;

namespace RMAZOR.Models.ItemProceeders
{
    public class DiodeEventArgs : MazeItemProceedInfoEventArgs
    {
        public DiodeEventArgs(IMazeItemProceedInfo _Info) : base(_Info)
        {
            
        }
    }

    public delegate void DiodeEventHandler(DiodeEventArgs _Args);

    public interface IDiodesProceeder : 
        IItemsProceeder, 
        ICharacterMoveStarted,
        ICharacterMoveContinued, 
        ICharacterMoveFinished,
        IGetAllProceedInfos
    {
        event DiodeEventHandler DiodeBlock;
        event DiodeEventHandler DiodePass;
    }
    
    public class DiodesProceeder : ItemsProceederBase, IDiodesProceeder
    {
        #region nonpublic members

        private IMazeItemProceedInfo m_DiodeOnCurrentCharacterMove;
        private bool                 m_DiodePassInvokedOnCurrentCharacterMove;

        #endregion

        #region inject
        
        protected override EMazeItemType[] Types => new[] {EMazeItemType.Diode};
        
        private DiodesProceeder(
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
        
        public event DiodeEventHandler      DiodeBlock;
        public event DiodeEventHandler      DiodePass;
        public Func<IMazeItemProceedInfo[]> GetAllProceedInfos { get; set; }
        
        public void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            m_DiodePassInvokedOnCurrentCharacterMove = false;
            m_DiodeOnCurrentCharacterMove = null;
            var fullPath = RmazorUtils.GetFullPath(_Args.From, _Args.To)
                .Except(new[] {_Args.From, _Args.To});
            foreach (var pos in fullPath)
                foreach (var info in GetAllProceedInfos())
                    if (info.StartPosition == pos && info.Type == EMazeItemType.Diode)
                    {
                        m_DiodeOnCurrentCharacterMove = info;
                        break;
                    }
        }
        
        public void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args)
        {
            if (m_DiodePassInvokedOnCurrentCharacterMove)
                return;
            if (m_DiodeOnCurrentCharacterMove == null)
                return;
            if (_Args.Position != m_DiodeOnCurrentCharacterMove.StartPosition)
                return;
            var args = new DiodeEventArgs(m_DiodeOnCurrentCharacterMove);
            DiodePass?.Invoke(args);
            m_DiodePassInvokedOnCurrentCharacterMove = true;
        }

        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            if (_Args.BlockWhoStopped == null || _Args.BlockWhoStopped.Type != EMazeItemType.Diode)
                return;
            var args = new DiodeEventArgs(_Args.BlockWhoStopped);
            DiodeBlock?.Invoke(args);
        }

        #endregion
    }
}