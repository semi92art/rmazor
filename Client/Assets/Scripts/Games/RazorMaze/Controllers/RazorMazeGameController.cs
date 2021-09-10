using System.Linq;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Views;
using TimeProviders;
using UnityEngine;
using Utils;
using Zenject;

namespace Games.RazorMaze.Controllers
{
    public interface IGameManager : IInit, IPreInit, IPostInit { }
    
    public class RazorMazeGameController : MonoBehaviour, IGameManager
    {
        #region singleton

        private static RazorMazeGameController _instance;

        public static RazorMazeGameController Instance
        {
            get
            {
                if (_instance != null) return _instance;
                var go = CommonUtils.FindOrCreateGameObject("Game Manager", out bool _WasFound);
                _instance = _WasFound ? go.GetComponent<RazorMazeGameController>() : go.AddComponent<RazorMazeGameController>();
                return _instance;
            }
        }

        #endregion

        #region inject
        
        private IModelGame           Model { get; set; }
        private IViewGame            View { get; set; }
        
        [Inject]
        public void Inject(IModelGame _ModelGame, IViewGame _ViewGame)
        {
            Model = _ModelGame;
            View = _ViewGame;
        }
        
        #endregion
        
        #region api

        public event NoArgsHandler PreInitialized;
        public event NoArgsHandler Initialized;
        public event NoArgsHandler PostInitialized;
        
        public void PreInit()
        {
            GameTimeProvider.Instance.Reset();

            var rotation                                        = Model.MazeRotation;
            var pathItemsProceeder                              = Model.PathItemsProceeder;
            var movingItemsProceeder                            = Model.MovingItemsProceeder;
            var gravityItemsProceeder                           = Model.GravityItemsProceeder;
            var trapsReactProceeder                             = Model.TrapsReactProceeder;
            var trapsIncreasingProceeder                        = Model.TrapsIncreasingProceeder;
            var portalsProceeder                                = Model.PortalsProceeder;
            var turretsProceeder                                = Model.TurretsProceeder;
            var shredingerProceeder                             = Model.ShredingerBlocksProceeder;
            var springboardProceeder                            = Model.SpringboardProceeder;
            var character                                       = Model.Character;
            var scoring                                         = Model.Scoring;
            var levelStaging                                    = Model.LevelStaging;
            
            pathItemsProceeder.PathProceedEvent                 += DataOnPathProceedEvent;
            
            rotation.RotationStarted                            += OnMazeRotationStarted;
            rotation.Rotation                                   += OnMazeRotation;
            rotation.RotationFinished                           += OnMazeRotationFinished;
            
            movingItemsProceeder.MazeItemMoveStarted            += OnMazeItemMoveStarted;
            movingItemsProceeder.MazeItemMoveContinued          += OnMazeItemMoveContinued;
            movingItemsProceeder.MazeItemMoveFinished           += OnMazeItemMoveFinished;

            gravityItemsProceeder.MazeItemMoveStarted           += OnMazeItemMoveStarted;
            gravityItemsProceeder.MazeItemMoveContinued         += OnMazeItemMoveContinued;
            gravityItemsProceeder.MazeItemMoveFinished          += OnMazeItemMoveFinished;
            
            trapsReactProceeder.TrapReactStageChanged           += OnMazeTrapReactStageChanged;
            trapsIncreasingProceeder.TrapIncreasingStageChanged += OnMazeTrapIncreasingStageChanged;
            turretsProceeder.TurretShoot                        += OnTurretShoot;
            portalsProceeder.PortalEvent                        += OnPortalEvent;
            shredingerProceeder.ShredingerBlockEvent            += OnShredingerBlockEvent;
            springboardProceeder.SpringboardEvent               += OnSpringboardEvent;

            character.HealthChanged                             += OnCharacterHealthChanged;
            character.Death                                     += OnCharacterDeath;
            character.CharacterMoveStarted                      += OnCharacterMoveStarted;
            character.CharacterMoveContinued                    += OnCharacterMoveContinued;
            character.CharacterMoveFinished                     += OnCharacterMoveFinished;
            
            scoring.ScoreChanged                                += OnScoreChanged;
            scoring.NecessaryScoreReached                       += OnNecessaryScoreReached;

            levelStaging.LevelBeforeStarted                     += OnBeforeLevelStarted;
            levelStaging.LevelStarted                           += OnLevelStarted;
            levelStaging.LevelFinished                          += OnLevelFinished;
            
            View.InputConfigurator.Command                      += OnInputCommand;
            View.MazeCommon.GameLoopUpdate                      += OnGameLoopUpdate;
            
            Model.PreInitialized += () => PreInitialized?.Invoke();
            Model.PreInit();
        }

        public void Init()
        {
            bool modelInitialized = false;
            bool viewInitialized = false;

            Model.Initialized += () => modelInitialized = true;
            View.Initialized  += () => viewInitialized = true;
            
            Model.Init();
            View.Init();

            System.Func<bool> allInitialized = () => modelInitialized && viewInitialized;
            
            Coroutines.Run(Coroutines.WaitWhile(
                () => !allInitialized.Invoke(),
                () => Initialized?.Invoke()));
        }

        

        #endregion

        #region event methods
        
        private void OnGameLoopUpdate() => Model.Data.OnGameLoopUpdate();
        private void DataOnPathProceedEvent(V2Int _PathItem) => View.MazeCommon.OnPathProceed(_PathItem);
        private void OnInputCommand(int _Value) => Model.InputScheduler.AddCommand((EInputCommand)_Value);
        private void OnCharacterHealthChanged(HealthPointsEventArgs _Args) => View.Character.OnHealthChanged(_Args);
        private void OnCharacterDeath() => View.Character.OnDeath();
        private void OnCharacterMoveStarted(CharacterMovingEventArgs _Args) => View.Character.OnMovingStarted(_Args);

        private void OnCharacterMoveContinued(CharacterMovingEventArgs _Args) => View.Character.OnMoving(_Args);
        private void OnCharacterMoveFinished(CharacterMovingEventArgs _Args) => View.Character.OnMovingFinished(_Args);
        
        private void OnMazeRotationStarted(MazeRotateDirection _Direction, MazeOrientation _Orientation) => View.MazeRotation.StartRotation(_Direction, _Orientation);
        private void OnMazeRotation(float _Progress) => View.MazeRotation.Rotate(_Progress);
        private void OnMazeRotationFinished(MazeRotateDirection _Direction, MazeOrientation _Orientation) => View.MazeRotation.FinishRotation();
        
        private void OnMazeItemMoveStarted(MazeItemMoveEventArgs _Args)
        {
            View.MazeMovingItemsGroup.OnMazeItemMoveStarted(_Args);
            if (_Args.Item.Type == EMazeItemType.GravityBlock)
                View.InputConfigurator.Locked = true;
        }

        private void OnMazeItemMoveContinued(MazeItemMoveEventArgs _Args) => View.MazeMovingItemsGroup.OnMazeItemMoveContinued(_Args);
        private void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args)
        {
            View.MazeMovingItemsGroup.OnMazeItemMoveFinished(_Args);
            if (_Args.Item.Type == EMazeItemType.GravityBlock)
                View.InputConfigurator.Locked = false;
        }

        private void OnMazeTrapReactStageChanged(MazeItemTrapReactEventArgs _Args) => View.MazeTrapsReactItemsGroup.OnMazeTrapReactStageChanged(_Args);
        private void OnMazeTrapIncreasingStageChanged(MazeItemTrapIncreasingEventArgs _Args) => View.MazeTrapsIncreasingItemsGroup.OnMazeTrapIncreasingStageChanged(_Args);
        private void OnTurretShoot(TurretShotEventArgs _Args) => View.MazeTurretsGroup.OnTurretShoot(_Args);
        private void OnPortalEvent(PortalEventArgs _Args) => View.PortalsGroup.OnPortalEvent(_Args);
        private void OnShredingerBlockEvent(ShredingerBlockArgs _Args) => View.ShredingerBlocksGroup.OnShredingerBlockEvent(_Args);
        private void OnSpringboardEvent(SpringboardEventArgs _Args) => View.SpringboardItemsGroup.OnSpringboardEvent(_Args);

        
        public void SetMazeInfo(MazeInfo _Info) => Model.Data.Info = _Info;

        #endregion
    
        #region nonpublic methods

        protected virtual void OnBeforeLevelStarted(LevelStateChangedArgs _Args)
        {
            Model.Scoring.Score = 0;
            View.UI?.OnBeforeLevelStarted(_Args, () => Model.LevelStaging.StartLevel());
        }

        protected virtual void OnLevelStarted(LevelStateChangedArgs _Args) => View.UI?.OnLevelStarted(_Args);

        protected virtual void OnLevelFinished(LevelStateChangedArgs _Args)
        {
            View.UI?.OnLevelFinished(_Args, () =>
                {
                    Model.LevelStaging.Level++;
                    Model.LevelStaging.BeforeStartLevel();
                });
        }

        private void OnScoreChanged(int _Score) => throw new System.NotImplementedException();
        protected virtual void OnNecessaryScoreReached() => Model.LevelStaging.FinishLevel();

        #endregion
    
        #region engine methods

        protected virtual void OnDestroy()
        {
            var rotation                                        = Model.MazeRotation;
            var movingItemsProceeder                            = Model.MovingItemsProceeder;
            var gravityItemsProceeder                           = Model.GravityItemsProceeder;
            var trapsReactProceeder                             = Model.TrapsReactProceeder;
            var trapsIncreasingProceeder                        = Model.TrapsIncreasingProceeder;
            var portalsProceeder                                = Model.PortalsProceeder;
            var turretsProceeder                                = Model.TurretsProceeder;
            var shredingerProceeder                             = Model.ShredingerBlocksProceeder;
            var springboardProceeder                            = Model.SpringboardProceeder;
            var character                                       = Model.Character;
            var scoring                                         = Model.Scoring;
            var levelStaging                                    = Model.LevelStaging;
            
            rotation.RotationStarted                            -= OnMazeRotationStarted;
            rotation.Rotation                                   -= OnMazeRotation;
            rotation.RotationFinished                           -= OnMazeRotationFinished;
            
            movingItemsProceeder.MazeItemMoveStarted            -= OnMazeItemMoveStarted;
            movingItemsProceeder.MazeItemMoveContinued          -= OnMazeItemMoveContinued;
            movingItemsProceeder.MazeItemMoveFinished           -= OnMazeItemMoveFinished;

            gravityItemsProceeder.MazeItemMoveStarted           -= OnMazeItemMoveStarted;
            gravityItemsProceeder.MazeItemMoveContinued         -= OnMazeItemMoveContinued;
            gravityItemsProceeder.MazeItemMoveFinished          -= OnMazeItemMoveFinished;
            
            trapsReactProceeder.TrapReactStageChanged           -= OnMazeTrapReactStageChanged;
            trapsIncreasingProceeder.TrapIncreasingStageChanged -= OnMazeTrapIncreasingStageChanged;
            turretsProceeder.TurretShoot                        -= OnTurretShoot;
            portalsProceeder.PortalEvent                        -= OnPortalEvent;
            shredingerProceeder.ShredingerBlockEvent            -= OnShredingerBlockEvent;
            springboardProceeder.SpringboardEvent               -= OnSpringboardEvent;

            character.HealthChanged                             -= OnCharacterHealthChanged;
            character.Death                                     -= OnCharacterDeath;
            character.CharacterMoveStarted                      -= OnCharacterMoveStarted;
            character.CharacterMoveContinued                    -= OnCharacterMoveContinued;
            character.CharacterMoveFinished                     -= OnCharacterMoveFinished;
            
            scoring.ScoreChanged                                -= OnScoreChanged;
            scoring.NecessaryScoreReached                       -= OnNecessaryScoreReached;

            levelStaging.LevelBeforeStarted                     -= OnBeforeLevelStarted;
            levelStaging.LevelStarted                           -= OnLevelStarted;
            levelStaging.LevelFinished                          -= OnLevelFinished;
            
            View.InputConfigurator.Command                      -= OnInputCommand;
        }

        #endregion

        public void PostInit()
        {
            Model.Data.ProceedingControls = true;
            Model.Data.ProceedingMazeItems = true;
        }


    }
}