using System.Linq;
using Common.Enums;
using Common.Helpers;

namespace Common.UI.DialogViewers
{
    public enum EDialogViewerType
    {
        Fullscreen,
        Medium1,
        Medium2,
        Medium3
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
        private IDialogViewerMedium3    DialogViewerMedium3    { get; }

        public DialogViewersController(
            IViewUICanvasGetter     _CanvasGetter,
            IDialogViewerFullscreen _DialogViewerFullscreen,
            IDialogViewerMedium1    _DialogViewerMedium1,
            IDialogViewerMedium2    _DialogViewerMedium2,
            IDialogViewerMedium3    _DialogViewerMedium3)
        {
            CanvasGetter           = _CanvasGetter;
            DialogViewerFullscreen = _DialogViewerFullscreen;
            DialogViewerMedium1    = _DialogViewerMedium1;
            DialogViewerMedium2    = _DialogViewerMedium2;
            DialogViewerMedium3    = _DialogViewerMedium3;
        }

        #endregion

        #region api

        public override void Init()
        {
            CanvasGetter          .Init();
            DialogViewerFullscreen.Init();
            DialogViewerMedium1   .Init();
            DialogViewerMedium2   .Init();
            DialogViewerMedium3   .Init();
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
                EDialogViewerType.Medium3     => DialogViewerMedium3,
                _                             => null
            };
        }

        #endregion

        #region nonpublic methods

        private void SetOtherDialogViewersShowingActions()
        {
            DialogViewerFullscreen.OtherDialogViewersShowing = () =>
            {
                var panels = new[]
                {
                    DialogViewerMedium1.CurrentPanel,
                    DialogViewerMedium2.CurrentPanel,
                    DialogViewerMedium3.CurrentPanel
                };
                return panels.Any(_P => _P != null &&
                                        _P.AppearingState != EAppearingState.Dissapeared);
            };
            DialogViewerMedium1.OtherDialogViewersShowing = () =>
            {
                var panels = new[]
                {
                    DialogViewerFullscreen.CurrentPanel,
                    DialogViewerMedium2   .CurrentPanel,
                    DialogViewerMedium3   .CurrentPanel
                };
                return panels.Any(_P => _P != null &&
                                        _P.AppearingState != EAppearingState.Dissapeared);
            };
            DialogViewerMedium2.OtherDialogViewersShowing = () =>
            {
                var panels = new[]
                {
                    DialogViewerMedium1   .CurrentPanel,
                    DialogViewerFullscreen.CurrentPanel,
                    DialogViewerMedium3   .CurrentPanel
                };
                return panels.Any(_P => _P != null &&
                                        _P.AppearingState != EAppearingState.Dissapeared);
            };
            DialogViewerMedium3.OtherDialogViewersShowing = () =>
            {
                var panels = new[]
                {
                    DialogViewerMedium1   .CurrentPanel,
                    DialogViewerMedium2   .CurrentPanel,
                    DialogViewerFullscreen.CurrentPanel
                };
                return panels.Any(_P => _P != null &&
                                        _P.AppearingState != EAppearingState.Dissapeared);
            };
        }

        #endregion
    }
}