using Common.Enums;
using Common.Helpers;

namespace Common.UI.DialogViewers
{
    public enum EDialogViewerType
    {
        Fullscreen,
        Medium1,
        Medium2
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
        private IDialogViewerFullscreen DialogViewerFullscreen { get; }
        private IDialogViewerMedium1    DialogViewerMedium1    { get; }
        private IDialogViewerMedium2    DialogViewerMedium2    { get; }

        public DialogViewersController(
            IViewUICanvasGetter     _CanvasGetter,
            IDialogViewerFullscreen _DialogViewerFullscreen,
            IDialogViewerMedium1   _DialogViewerMedium1,
            IDialogViewerMedium2 _DialogViewerMedium2)
        {
            CanvasGetter           = _CanvasGetter;
            DialogViewerFullscreen = _DialogViewerFullscreen;
            DialogViewerMedium1   = _DialogViewerMedium1;
            DialogViewerMedium2 = _DialogViewerMedium2;
        }

        #endregion

        #region api

        public override void Init()
        {
            CanvasGetter.Init();
            DialogViewerFullscreen.Init();
            DialogViewerMedium1.Init();
            DialogViewerMedium2.Init();
            SetOtherDialogViewersShowingActions();
            base.Init();
        }
        
        public IDialogViewer GetViewer(EDialogViewerType _DialogViewerType)
        {
            return _DialogViewerType switch
            {
                EDialogViewerType.Fullscreen  => DialogViewerFullscreen,
                EDialogViewerType.Medium1     => DialogViewerMedium1,
                EDialogViewerType.Medium2     => DialogViewerMedium2,
                _                             => null
            };
        }

        #endregion

        #region nonpublic methods

        private void SetOtherDialogViewersShowingActions()
        {
            DialogViewerFullscreen.OtherDialogViewersShowing = () =>
            {
                var panel1 = DialogViewerMedium1.CurrentPanel;
                var panel2 = DialogViewerMedium2.CurrentPanel;
                return (panel1 != null && 
                       panel1.AppearingState != EAppearingState.Dissapeared)
                    || (panel2 != null && 
                       panel2.AppearingState != EAppearingState.Dissapeared);
            };
            DialogViewerMedium1.OtherDialogViewersShowing = () =>
            {
                var panel1 = DialogViewerFullscreen.CurrentPanel;
                var panel2 = DialogViewerMedium2.CurrentPanel;
                return (panel1 != null && 
                        panel1.AppearingState != EAppearingState.Dissapeared)
                       || (panel2 != null && 
                           panel2.AppearingState != EAppearingState.Dissapeared);
            };
            DialogViewerMedium2.OtherDialogViewersShowing = () =>
            {
                var panel1 = DialogViewerFullscreen.CurrentPanel;
                var panel2 = DialogViewerMedium1.CurrentPanel;
                return (panel1 != null && 
                        panel1.AppearingState != EAppearingState.Dissapeared)
                       || (panel2 != null && 
                           panel2.AppearingState != EAppearingState.Dissapeared);
            };
        }

        #endregion
    }
}