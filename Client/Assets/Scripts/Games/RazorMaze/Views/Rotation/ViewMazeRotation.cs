using System.Collections;
using System.Linq;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Characters;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.InputConfigurators;
using Ticker;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Games.RazorMaze.Views.Rotation
{
    public class ViewMazeRotation : ViewMazeRotationBase, IUpdateTick
    {
        #region nonpublic members

        private Rigidbody2D m_Rb;
        private bool        m_EnableRotation;
        private bool        m_IsHighlightingRotationPossibility;
        private float       m_HighlightRotationPossibilityTimer;

        #endregion
        
        #region inject
        
        private ViewSettings                ViewSettings        { get; }
        private IContainersGetter           ContainersGetter    { get; }
        private IViewGameTicker             GameTicker          { get; }
        private IModelGame                  Model               { get; }
        private IViewCharacter              Character           { get; }
        private IViewInputCommandsProceeder CommandsProceeder   { get; }

        public ViewMazeRotation(
            ViewSettings _ViewSettings,
            IContainersGetter _ContainersGetter, 
            IViewGameTicker _GameTicker,
            IModelGame _Model,
            IViewCharacter _Character,
            IViewInputCommandsProceeder _CommandsProceeder)
        {
            ViewSettings = _ViewSettings;
            ContainersGetter = _ContainersGetter;
            GameTicker = _GameTicker;
            Model = _Model;
            Character = _Character;
            CommandsProceeder = _CommandsProceeder;
        }
        
        #endregion

        #region api
        
        public override event UnityAction<float> RotationContinued;
        public override event MazeOrientationHandler RotationFinished;

        public override void Init()
        {
            var cont = ContainersGetter.GetContainer(ContainerNames.MazeHolder);
            m_Rb = cont.gameObject.AddComponent<Rigidbody2D>();
            m_Rb.gravityScale = 0;
            m_Rb.constraints = RigidbodyConstraints2D.FreezePosition;
            GameTicker.Register(this);
            CommandsProceeder.Command += OnCommand;
            base.Init();
        }

        private void OnCommand(EInputCommand _Command, object[] _Args)
        {
            if (!RazorMazeUtils.GetMoveCommands().Concat(RazorMazeUtils.GetRotateCommands()).Contains(_Command))
                return;
            m_IsHighlightingRotationPossibility = false;
        }

        public override void OnRotationStarted(MazeRotationEventArgs _Args)
        {
            if (_Args.Instantly)
                m_Rb.transform.eulerAngles = Vector3.zero;
            else
                Coroutines.Run(RotationCoroutine(_Args));
        }

        public override void OnRotationFinished(MazeRotationEventArgs _Args)
        {
            Coroutines.Run(Coroutines.Delay(
                () => RazorMazeUtils.UnlockCommandsOnRotationFinished(CommandsProceeder),
                0.5f));
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    m_EnableRotation = Model.GetAllProceedInfos().Any(_Info =>
                    _Info.Type == EMazeItemType.GravityBlock
                    || _Info.Type == EMazeItemType.GravityTrap
                    || _Info.Type == EMazeItemType.GravityBlockFree);
                    if (MazeContainsGravityItems())
                    {
                        m_IsHighlightingRotationPossibility = true;
                        m_HighlightRotationPossibilityTimer = 0f;
                    }
                    break;
                case ELevelStage.ReadyToStart:
                    if (_Args.PreviousStage != ELevelStage.Loaded)
                        return;
                    if (!m_EnableRotation)
                    {
                        CommandsProceeder.LockCommand(EInputCommand.RotateClockwise);
                        CommandsProceeder.LockCommand(EInputCommand.RotateCounterClockwise);
                    }
                    break;
                case ELevelStage.ReadyToUnloadLevel:
                    break;
            }
        }
        
        public void UpdateTick()
        {
            const float timerThreshold = 1.5f;
            if (!m_IsHighlightingRotationPossibility) 
                return;
            m_HighlightRotationPossibilityTimer += GameTicker.DeltaTime;
            if (m_HighlightRotationPossibilityTimer < timerThreshold)
                return;
            m_HighlightRotationPossibilityTimer = 0f;
            Coroutines.Run(HighlightRotationPossibilityCoroutine());
        }

        #endregion

        #region nonpublic methods

        private IEnumerator RotationCoroutine(MazeRotationEventArgs _Args)
        {
            GetRotationParams(_Args,
                out float startAngle,
                out float endAngle,
                out float rotationAngleDistance,
                out float realSkidAngle);
            
            yield return Coroutines.Lerp(
                startAngle,
                endAngle + realSkidAngle,
                rotationAngleDistance / (90f * ViewSettings.MazeRotationSpeed),
                _Angle => m_Rb.transform.rotation = Quaternion.Euler(0f, 0f, _Angle),
                GameTicker, 
                (_, __) =>
                {
                    RotationFinished?.Invoke(_Args);
                    Coroutines.Run(Coroutines.Lerp(
                        endAngle + realSkidAngle,
                        endAngle,
                        1 / (ViewSettings.MazeRotationSpeed * 2f),
                        _Angle => m_Rb.transform.rotation = Quaternion.Euler(0f, 0f, _Angle),
                        GameTicker,
                        (___, ____) => Character.OnRotationAfterFinished(_Args)));
                });
        }

        private void GetRotationParams(MazeRotationEventArgs _Args, 
            out float _StartAngle,
            out float _EndAngle,
            out float _RotationAngleDistance,
            out float _RealSkidAngle)
        {
            _StartAngle = GetAngleByOrientation(_Args.CurrentOrientation);
            float dirCoeff = _Args.Direction == EMazeRotateDirection.Clockwise ? -1 : 1;
            _RotationAngleDistance = GetRotationAngleDistance(_Args);
            _EndAngle = _StartAngle + _RotationAngleDistance * dirCoeff;
            _RealSkidAngle = 3f * dirCoeff;
        }

        private static float GetRotationAngleDistance(MazeRotationEventArgs _Args)
        {
            var dir = _Args.Direction;
            var from = (int)_Args.CurrentOrientation;
            var to = (int)_Args.NextOrientation;

            if (from == to)
                return 0f;
            if (dir == EMazeRotateDirection.Clockwise && to < from)
                return 90f * (4 - from + to);
            if (dir == EMazeRotateDirection.CounterClockwise && to > from)
                return 90f * (4 - to + from);
            return 90f * Mathf.Abs(to - from);
        }
        
        private bool MazeContainsGravityItems()
        {
            return Model.GetAllProceedInfos()
                .Any(_Info => RazorMazeUtils.GravityItemTypes().Contains(_Info.Type));
        }

        private IEnumerator HighlightRotationPossibilityCoroutine()
        {
            var fakeArgs = new MazeRotationEventArgs(
                EMazeRotateDirection.Clockwise, 
                MazeOrientation.North,
                MazeOrientation.East, 
                false);
            GetRotationParams(fakeArgs, 
                out float startAngle,
                out _,
                out _,
                out float realSkidAngle);
            realSkidAngle *= 2f;
            void SetRotation(float _Angle)
            {
                m_Rb.transform.rotation = Quaternion.Euler(0f, 0f, _Angle);
            }
            bool BreakPredicate()
            {
                return !m_IsHighlightingRotationPossibility;
            }
            yield return Coroutines.Lerp(
                startAngle,
                startAngle + realSkidAngle,
                1 / (ViewSettings.MazeRotationSpeed * 2f),
                SetRotation,
                GameTicker,
                (_, __) =>
                {
                    Coroutines.Run(Coroutines.Lerp(
                        startAngle + realSkidAngle,
                        startAngle - realSkidAngle,
                        1f / ViewSettings.MazeRotationSpeed,
                            SetRotation,
                            GameTicker,
                        (_, __) =>
                        {
                            Coroutines.Run(Coroutines.Lerp(
                                startAngle - realSkidAngle,
                                startAngle,
                                1f / (ViewSettings.MazeRotationSpeed * 2f),
                                SetRotation,
                                GameTicker,
                                _BreakPredicate: BreakPredicate));
                        },
                        BreakPredicate));
                },
                BreakPredicate);
        }

        #endregion
    }
}