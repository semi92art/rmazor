using Common.Enums;
using Common.Extensions;
using Common.Helpers;

namespace Common.UI
{
    public enum EDialogViewerType
    {
        Fullscreen,
        Medium
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
        private IDialogViewerMedium   DialogViewerMedium   { get; }

        public DialogViewersController(
            IViewUICanvasGetter     _CanvasGetter,
            IFullscreenDialogViewer _FullscreenDialogViewer,
            IDialogViewerMedium   _DialogViewerMedium)
        {
            CanvasGetter           = _CanvasGetter;
            FullscreenDialogViewer = _FullscreenDialogViewer;
            DialogViewerMedium   = _DialogViewerMedium;
        }

        #endregion

        #region api

        public override void Init()
        {
            CanvasGetter.Init();
            var parent = CanvasGetter.GetCanvas().RTransform();
            FullscreenDialogViewer.Init();
            DialogViewerMedium.Init();
            SetOtherDialogViewersShowingActions();
            base.Init();
        }
        
        public IDialogViewer GetViewer(EDialogViewerType _DialogViewerType)
        {
            return _DialogViewerType switch
            {
                EDialogViewerType.Fullscreen      => FullscreenDialogViewer,
                EDialogViewerType.Medium => DialogViewerMedium,
                _                          => null
            };
        }

        #endregion

        #region nonpublic methods

        private void SetOtherDialogViewersShowingActions()
        {
            FullscreenDialogViewer.OtherDialogViewersShowing = () =>
            {
                var panel = DialogViewerMedium.CurrentPanel;
                return panel != null && 
                       panel.AppearingState != EAppearingState.Dissapeared;
            };
            DialogViewerMedium.OtherDialogViewersShowing = () =>
            {
                var panel = FullscreenDialogViewer.CurrentPanel;
                return panel != null && 
                       panel.AppearingState != EAppearingState.Dissapeared;
            };
        }

        #endregion
    }
}