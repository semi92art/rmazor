using Common;
using Common.Helpers;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views;
using Zenject;

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
        private CommonGameSettings   Settings            { get; set; }

        [Inject]
        public void Inject(
            IModelGame           _Model,
            IViewGame            _View,
            CommonGameSettings   _Settings)
        {
            Model               = _Model;
            View                = _View;
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
            
            Model.GravityItemsProceeder.MazeItemMoveStarted           += View.MazeItemsGroupSet.GravityItemsGroup.OnMazeItemMoveStarted;
            Model.GravityItemsProceeder.MazeItemMoveStarted           += View.UI.GameControls.OnMazeItemMoveStarted;
            Model.GravityItemsProceeder.MazeItemMoveContinued         += View.MazeItemsGroupSet.GravityItemsGroup.OnMazeItemMoveContinued;
            Model.GravityItemsProceeder.MazeItemMoveFinished          += View.MazeItemsGroupSet.GravityItemsGroup.OnMazeItemMoveFinished;
            Model.GravityItemsProceeder.MazeItemMoveFinished          += View.UI.GameControls.OnMazeItemMoveFinished;
            
            Model.TrapsMovingProceeder.MazeItemMoveStarted            += View.MazeItemsGroupSet.MovingItemsGroup.OnMazeItemMoveStarted;
            Model.TrapsMovingProceeder.MazeItemMoveStarted            += View.UI.GameControls.OnMazeItemMoveStarted;
            Model.TrapsMovingProceeder.MazeItemMoveContinued          += View.MazeItemsGroupSet.MovingItemsGroup.OnMazeItemMoveContinued;
            Model.TrapsMovingProceeder.MazeItemMoveFinished           += View.MazeItemsGroupSet.MovingItemsGroup.OnMazeItemMoveFinished;
            Model.TrapsMovingProceeder.MazeItemMoveFinished           += View.UI.GameControls.OnMazeItemMoveFinished;
            
            Model.TrapsReactProceeder.TrapReactStageChanged           += View.MazeItemsGroupSet.TrapsReactItemsGroup.OnMazeTrapReactStageChanged;
            Model.TrapsIncreasingProceeder.TrapIncreasingStageChanged += View.MazeItemsGroupSet.TrapsIncItemsGroup.OnMazeTrapIncreasingStageChanged;
            Model.TurretsProceeder.TurretShoot                        += View.MazeItemsGroupSet.TurretsGroup.OnTurretShoot;
            Model.PortalsProceeder.PortalEvent                        += View.MazeItemsGroupSet.PortalsGroup.OnPortalEvent;
            Model.ShredingerBlocksProceeder.ShredingerBlockEvent      += View.MazeItemsGroupSet.ShredingerBlocksGroup.OnShredingerBlockEvent;
            Model.SpringboardProceeder.SpringboardEvent               += View.MazeItemsGroupSet.SpringboardItemsGroup.OnSpringboardEvent;
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
            SRDebug.Init();
            SROptions.Init(Model.Settings, View.Settings, Model.LevelStaging, View.Managers, View.CommandsProceeder);
            Dbg.Log("SR Debugger Initialized");
            View.Managers.DebugManager.Init();
            Dbg.Log("Debug Manager Initialized");
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
            
            Model.GravityItemsProceeder.MazeItemMoveStarted           -= View.MazeItemsGroupSet.MovingItemsGroup.OnMazeItemMoveStarted;
            Model.GravityItemsProceeder.MazeItemMoveStarted           -= View.UI.GameControls.OnMazeItemMoveStarted;
            Model.GravityItemsProceeder.MazeItemMoveContinued         -= View.MazeItemsGroupSet.MovingItemsGroup.OnMazeItemMoveContinued;
            Model.GravityItemsProceeder.MazeItemMoveFinished          -= View.MazeItemsGroupSet.MovingItemsGroup.OnMazeItemMoveFinished;
            Model.GravityItemsProceeder.MazeItemMoveFinished          -= View.UI.GameControls.OnMazeItemMoveFinished;
            
            Model.TrapsMovingProceeder.MazeItemMoveStarted            -= View.MazeItemsGroupSet.MovingItemsGroup.OnMazeItemMoveStarted;
            Model.TrapsMovingProceeder.MazeItemMoveStarted            -= View.UI.GameControls.OnMazeItemMoveStarted;
            Model.TrapsMovingProceeder.MazeItemMoveContinued          -= View.MazeItemsGroupSet.MovingItemsGroup.OnMazeItemMoveContinued;
            Model.TrapsMovingProceeder.MazeItemMoveFinished           -= View.MazeItemsGroupSet.MovingItemsGroup.OnMazeItemMoveFinished;
            Model.TrapsMovingProceeder.MazeItemMoveFinished           -= View.UI.GameControls.OnMazeItemMoveFinished;
            
            Model.TrapsReactProceeder.TrapReactStageChanged           -= View.MazeItemsGroupSet.TrapsReactItemsGroup.OnMazeTrapReactStageChanged;
            Model.TrapsIncreasingProceeder.TrapIncreasingStageChanged -= View.MazeItemsGroupSet.TrapsIncItemsGroup.OnMazeTrapIncreasingStageChanged;
            Model.TurretsProceeder.TurretShoot                        -= View.MazeItemsGroupSet.TurretsGroup.OnTurretShoot;
            Model.PortalsProceeder.PortalEvent                        -= View.MazeItemsGroupSet.PortalsGroup.OnPortalEvent;
            Model.ShredingerBlocksProceeder.ShredingerBlockEvent      -= View.MazeItemsGroupSet.ShredingerBlocksGroup.OnShredingerBlockEvent;
            Model.SpringboardProceeder.SpringboardEvent               -= View.MazeItemsGroupSet.SpringboardItemsGroup.OnSpringboardEvent;
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