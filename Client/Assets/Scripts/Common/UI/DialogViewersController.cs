﻿using Common.Enums;
using Common.Extensions;
using Common.Helpers;

namespace Common.UI
{
    public enum EDialogViewerType
    {
        Fullscreen,
        Proposal
    }
    
    public interface IDialogViewersController : IInit
    {
        IDialogViewer GetViewer(EDialogViewerType _DialogViewerType);
    }

    public class DialogViewersControllerFake : InitBase, IDialogViewersController
    {
        public IDialogViewer GetViewer(EDialogViewerType _DialogViewerType)
        {
            return null;
        }
    }
    
    public class DialogViewersController : InitBase, IDialogViewersController
    {
        #region inject

        private IViewUICanvasGetter     CanvasGetter           { get; }
        private IFullscreenDialogViewer FullscreenDialogViewer { get; }
        private IProposalDialogViewer   ProposalDialogViewer   { get; }

        public DialogViewersController(
            IViewUICanvasGetter     _CanvasGetter,
            IFullscreenDialogViewer _FullscreenDialogViewer,
            IProposalDialogViewer   _ProposalDialogViewer)
        {
            CanvasGetter           = _CanvasGetter;
            FullscreenDialogViewer = _FullscreenDialogViewer;
            ProposalDialogViewer   = _ProposalDialogViewer;
        }

        #endregion

        #region api

        public override void Init()
        {
            CanvasGetter.Init();
            var parent = CanvasGetter.GetCanvas().RTransform();
            FullscreenDialogViewer.Init();
            ProposalDialogViewer.Init();
            SetOtherDialogViewersShowingActions();
            base.Init();
        }
        
        public IDialogViewer GetViewer(EDialogViewerType _DialogViewerType)
        {
            return _DialogViewerType switch
            {
                EDialogViewerType.Fullscreen      => FullscreenDialogViewer,
                EDialogViewerType.Proposal => ProposalDialogViewer,
                _                          => null
            };
        }

        #endregion

        #region nonpublic methods

        private void SetOtherDialogViewersShowingActions()
        {
            FullscreenDialogViewer.OtherDialogViewersShowing = () =>
            {
                var panel = ProposalDialogViewer.CurrentPanel;
                return panel != null && 
                       panel.AppearingState != EAppearingState.Dissapeared;
            };
            ProposalDialogViewer.OtherDialogViewersShowing = () =>
            {
                var panel = FullscreenDialogViewer.CurrentPanel;
                return panel != null && 
                       panel.AppearingState != EAppearingState.Dissapeared;
            };
        }

        #endregion
    }
}