using Common;
using Common.Helpers;
using Common.Utils;
using GameHelpers;
using Managers;
using RMAZOR.Models;
using RMAZOR.Views;
using RMAZOR.Views.InputConfigurators;

namespace RMAZOR.Controllers
{
    public interface IGameController : IInit
    {
        IModelGame Model { get; }
        IViewGame  View  { get; }
    }
    
    public class GameController : MonoBehInitBase, IGameController
    {
        #region factory
        
        public static IGameController CreateInstance()
        {
            var go = CommonUtils.FindOrCreateGameObject("Game Manager", out bool wasFound);
            var instance = wasFound ? go.GetComponent<GameController>() 
                : go.AddComponent<GameController>();
            return instance;
        }

        #endregion

        #region inject
        
        public  IModelGame           Model               { get; private set; }
        public  IViewGame            View                { get; private set; }
        private IRemoteConfigManager RemoteConfigManager { get; set; }
        private CommonGameSettings   Settings            { get; set; }

        [Zenject.Inject]
        public void Inject(
            IModelGame           _Model,
            IViewGame            _View,
            IRemoteConfigManager _RemoteConfigManager,
            CommonGameSettings   _Settings)
        {
            Model               = _Model;
            View                = _View;
            RemoteConfigManager = _RemoteConfigManager;
            Settings            = _Settings;
        }

        #endregion
        
        #region api
        
        public override void Init()
        {
            InitDebugging();
            
            bool modelInitialized = false;
            bool viewInitialized = false;
            Model.Initialize += () => modelInitialized = true;
            View.Initialize += () => viewInitialized = true;
            
            Model.Init();

            Model.PathItemsProceeder.AllPathsProceededEvent           += View.Character.OnAllPathProceed;
            Model.PathItemsProceeder.AllPathsProceededEvent           += View.LevelStageController.OnAllPathProceed;
            Model.PathItemsProceeder.PathProceedEvent                 += View.PathItemsGroup.OnPathProceed;
            Model.MazeRotation.RotationStarted                        += View.MazeRotation.OnRotationStarted;
            Model.MazeRotation.RotationFinished                       += View.MazeRotation.OnRotationFinished;
            Model.MazeRotation.RotationFinished                       += View.InputController.TouchProceeder.OnRotationFinished;
            
            Model.GravityItemsProceeder.MazeItemMoveStarted           += View.GravityItemsGroup.OnMazeItemMoveStarted;
            Model.GravityItemsProceeder.MazeItemMoveStarted           += View.UI.GameControls.OnMazeItemMoveStarted;
            Model.GravityItemsProceeder.MazeItemMoveContinued         += View.GravityItemsGroup.OnMazeItemMoveContinued;
            Model.GravityItemsProceeder.MazeItemMoveFinished          += View.GravityItemsGroup.OnMazeItemMoveFinished;
            Model.GravityItemsProceeder.MazeItemMoveFinished          += View.UI.GameControls.OnMazeItemMoveFinished;
            
            Model.TrapsMovingProceeder.MazeItemMoveStarted            += View.MovingItemsGroup.OnMazeItemMoveStarted;
            Model.TrapsMovingProceeder.MazeItemMoveStarted            += View.UI.GameControls.OnMazeItemMoveStarted;
            Model.TrapsMovingProceeder.MazeItemMoveContinued          += View.MovingItemsGroup.OnMazeItemMoveContinued;
            Model.TrapsMovingProceeder.MazeItemMoveFinished           += View.MovingItemsGroup.OnMazeItemMoveFinished;
            Model.TrapsMovingProceeder.MazeItemMoveFinished           += View.UI.GameControls.OnMazeItemMoveFinished;
            
            Model.TrapsReactProceeder.TrapReactStageChanged           += View.TrapsReactItemsGroup.OnMazeTrapReactStageChanged;
            Model.TrapsIncreasingProceeder.TrapIncreasingStageChanged += View.TrapsIncItemsGroup.OnMazeTrapIncreasingStageChanged;
            Model.TurretsProceeder.TurretShoot                        += View.TurretsGroup.OnTurretShoot;
            Model.PortalsProceeder.PortalEvent                        += View.PortalsGroup.OnPortalEvent;
            Model.ShredingerBlocksProceeder.ShredingerBlockEvent      += View.ShredingerBlocksGroup.OnShredingerBlockEvent;
            Model.SpringboardProceeder.SpringboardEvent               += View.SpringboardItemsGroup.OnSpringboardEvent;
            Model.Character.CharacterMoveStarted                      += View.OnCharacterMoveStarted;
            Model.Character.CharacterMoveContinued                    += View.OnCharacterMoveContinued;
            Model.Character.CharacterMoveFinished                     += View.OnCharacterMoveFinished;
            Model.LevelStaging.LevelStageChanged                      += View.OnLevelStageChanged;
            
            View.CommandsProceeder.Command                            += Model.InputScheduler.AddCommand;
            View.MazeRotation.RotationFinished                        += Model.MazeRotation.OnRotationFinished;
            
            View.Init();
            
            Cor.Run(Cor.WaitWhile(
                () => !modelInitialized || !viewInitialized,
                () => base.Init()));
        }

        #endregion

        #region nonpublic members

        private void InitDebugging()
        {
            if (RemoteConfigManager.Initialized)
                OnRemoteConfigManagerInitialized();
            else 
                RemoteConfigManager.Initialize += OnRemoteConfigManagerInitialized;
            // if (View.InputController.TouchProceeder is ViewInputTouchProceederWithSRDebugInit proceeder)
            //     proceeder.OnSrDebugInitialized = OnRemoteConfigManagerInitialized;
        }

        private void OnRemoteConfigManagerInitialized()
        {
            if (!Settings.DebugEnabled)
                return;
            SROptions.Init(Model, View);
            SRDebug.Init();
        }

        #endregion

        #region engine methods

        protected virtual void OnDestroy()
        {
            if (Model == null || View == null)
                return;
            Model.PathItemsProceeder.AllPathsProceededEvent           -= View.Character.OnAllPathProceed;
            Model.PathItemsProceeder.AllPathsProceededEvent           -= View.LevelStageController.OnAllPathProceed;
            Model.PathItemsProceeder.PathProceedEvent                 -= View.PathItemsGroup.OnPathProceed;
            Model.MazeRotation.RotationStarted                        -= View.MazeRotation.OnRotationStarted;
            Model.MazeRotation.RotationFinished                       += View.MazeRotation.OnRotationFinished;
            
            Model.GravityItemsProceeder.MazeItemMoveStarted           -= View.MovingItemsGroup.OnMazeItemMoveStarted;
            Model.GravityItemsProceeder.MazeItemMoveStarted           -= View.UI.GameControls.OnMazeItemMoveStarted;
            Model.GravityItemsProceeder.MazeItemMoveContinued         -= View.MovingItemsGroup.OnMazeItemMoveContinued;
            Model.GravityItemsProceeder.MazeItemMoveFinished          -= View.MovingItemsGroup.OnMazeItemMoveFinished;
            Model.GravityItemsProceeder.MazeItemMoveFinished          -= View.UI.GameControls.OnMazeItemMoveFinished;
            
            Model.TrapsMovingProceeder.MazeItemMoveStarted            -= View.MovingItemsGroup.OnMazeItemMoveStarted;
            Model.TrapsMovingProceeder.MazeItemMoveStarted            -= View.UI.GameControls.OnMazeItemMoveStarted;
            Model.TrapsMovingProceeder.MazeItemMoveContinued          -= View.MovingItemsGroup.OnMazeItemMoveContinued;
            Model.TrapsMovingProceeder.MazeItemMoveFinished           -= View.MovingItemsGroup.OnMazeItemMoveFinished;
            Model.TrapsMovingProceeder.MazeItemMoveFinished           -= View.UI.GameControls.OnMazeItemMoveFinished;
            
            Model.TrapsReactProceeder.TrapReactStageChanged           -= View.TrapsReactItemsGroup.OnMazeTrapReactStageChanged;
            Model.TrapsIncreasingProceeder.TrapIncreasingStageChanged -= View.TrapsIncItemsGroup.OnMazeTrapIncreasingStageChanged;
            Model.TurretsProceeder.TurretShoot                        -= View.TurretsGroup.OnTurretShoot;
            Model.PortalsProceeder.PortalEvent                        -= View.PortalsGroup.OnPortalEvent;
            Model.ShredingerBlocksProceeder.ShredingerBlockEvent      -= View.ShredingerBlocksGroup.OnShredingerBlockEvent;
            Model.SpringboardProceeder.SpringboardEvent               -= View.SpringboardItemsGroup.OnSpringboardEvent;
            Model.Character.CharacterMoveStarted                      -= View.OnCharacterMoveStarted;
            Model.Character.CharacterMoveContinued                    -= View.OnCharacterMoveContinued;
            Model.Character.CharacterMoveFinished                     -= View.OnCharacterMoveFinished;
            Model.LevelStaging.LevelStageChanged                      -= View.OnLevelStageChanged;
            
            View.CommandsProceeder.Command                            -= Model.InputScheduler.AddCommand;
            View.MazeRotation.RotationFinished                        -= Model.MazeRotation.OnRotationFinished;
        }

        #endregion
    }
}