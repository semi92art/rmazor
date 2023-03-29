using Common;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Debugging;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.UI.DialogViewers;
using mazing.common.Runtime.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.UI.Panels;

namespace RMAZOR.Views.UI
{
    public class ViewUI : ViewUIBase
    {
        #region inject

        private IDialogViewersController       DialogViewersController      { get; }
        private IDialogPanelsSet               DialogPanelsSet              { get; }
        private IViewUIRateGamePanelController RateGamePanelController      { get; }
        private IFpsCounter                    FpsCounter                   { get; }
        private ICameraProvider                CameraProvider               { get; }
        private IRemoteConfigManager           RemoteConfigManager          { get; }
        private IDialogViewerMediumCommon      DialogViewerMediumCommon     { get; }
        private IDialogViewerFullscreenCommon  DialogViewerFullscreenCommon { get; }
        private IDialogViewerMedium2           DialogViewerMedium2          { get; }
        private IDialogViewerFullscreen2       DialogViewerFullscreen2      { get; }
        private IViewUIInputCommandsInvoker    InputCommandsInvoker         { get; }

        private ViewUI(
            IDialogViewersController       _DialogViewersController,
            IViewUIGameControls            _GameControls,
            IDialogPanelsSet               _DialogPanelsSet,
            IViewUIRateGamePanelController _RateGamePanelController,
            IFpsCounter                    _FpsCounter,
            ICameraProvider                _CameraProvider,
            IRemoteConfigManager           _RemoteConfigManager,
            IDialogViewerMediumCommon      _DialogViewerMediumCommon,
            IDialogViewerFullscreenCommon  _DialogViewerFullscreenCommon,
            IDialogViewerMedium2           _DialogViewerMedium2,
            IDialogViewerFullscreen2       _DialogViewerFullscreen2,
            IViewUIInputCommandsInvoker    _InputCommandsInvoker)
            : base(_GameControls)
        {
            DialogViewersController      = _DialogViewersController;
            DialogPanelsSet              = _DialogPanelsSet;
            RateGamePanelController      = _RateGamePanelController;
            FpsCounter                   = _FpsCounter;
            CameraProvider               = _CameraProvider;
            RemoteConfigManager          = _RemoteConfigManager;
            DialogViewerMediumCommon     = _DialogViewerMediumCommon;
            DialogViewerFullscreenCommon = _DialogViewerFullscreenCommon;
            DialogViewerMedium2          = _DialogViewerMedium2;
            DialogViewerFullscreen2      = _DialogViewerFullscreen2;
            InputCommandsInvoker         = _InputCommandsInvoker;
        }

        #endregion
        
        #region api

        public override void Init()
        {
            DialogViewerFullscreen2.Initialize += () => DialogViewerFullscreen2.Container.parent.SetLocalPosZ(0f);
            RegisterDialogViewers();
            GameControls           .Init();
            InputCommandsInvoker   .Init();
            DialogViewersController.Init();
            RateGamePanelController.Init();
            CommonUtils.DoOnInitializedEx(RemoteConfigManager, InitFpsCounter);
            Cor.Run(Cor.WaitNextFrame(() => DialogPanelsSet.Init()));
            base.Init();
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            RateGamePanelController.OnLevelStageChanged(_Args);
            GameControls.OnLevelStageChanged(_Args);
        }

        public override void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            GameControls.OnCharacterMoveFinished(_Args);
        }

        public override void OnMazeRotationFinished(MazeRotationEventArgs _Args)
        {
            GameControls.OnMazeRotationFinished(_Args);
        }

        #endregion

        #region nonpbulic methods

        private void RegisterDialogViewers()
        {
            var viewers = new IDialogViewer[]
            {
                DialogViewerMediumCommon,
                DialogViewerFullscreenCommon,
                DialogViewerFullscreen2,
                DialogViewerMedium2,
            };
            foreach (var viewer in viewers)
                DialogViewersController.RegisterDialogViewer(viewer);
        }

        private void InitFpsCounter()
        {
            FpsCounter.Init();
            if (CameraProvider.Camera.IsNotNull())
                FpsCounter.OnActiveCameraChanged(CameraProvider.Camera);
            CameraProvider.ActiveCameraChanged += FpsCounter.OnActiveCameraChanged;
        }

        #endregion
    }
}