using System.Collections.Generic;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Extensions;
using Common.Providers;
using Common.Utils;
using RMAZOR.Models;
using UnityEngine;

namespace RMAZOR.Views.UI
{
    public class ViewUIRotationControlsDisplayRotationIndicator : IViewUIRotationControls
    {
        #region inject

        private IModelGame                    Model          { get; }
        private IColorProvider                ColorProvider  { get; }
        private IRotatingPossibilityIndicator Indicator      { get; }
        private ICameraProvider               CameraProvider { get; }
        private IViewUITutorial               Tutorial       { get; }

        private ViewUIRotationControlsDisplayRotationIndicator(
            IModelGame                    _Model,
            IColorProvider                _ColorProvider,
            IRotatingPossibilityIndicator _Indicator,
            ICameraProvider               _CameraProvider,
            IViewUITutorial               _Tutorial)
        {
            Model          = _Model;
            ColorProvider  = _ColorProvider;
            Indicator      = _Indicator;
            CameraProvider = _CameraProvider;
            Tutorial       = _Tutorial;
        }

        #endregion

        #region api
        
        public bool HasButtons => false;
        
        public void Init(Vector4 _Offsets)
        {
            Indicator.Name = "Rotating Indicator";
            Indicator.Init(_Offsets);
            var bounds = GraphicUtils.GetVisibleBounds(CameraProvider.Camera);
            Indicator.SetPosition(new Vector2(bounds.center.x, bounds.min.y + 8f));
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            Indicator.OnLevelStageChanged(_Args);
            switch (_Args.LevelStage)
            {
                case ELevelStage.ReadyToStart when
                    _Args.PreviousStage != ELevelStage.Paused
                    && RmazorUtils.MazeContainsGravityItems(Model.GetAllProceedInfos()):
                {
                    // var tutType = Tutorial.IsCurrentLevelTutorial(out _);
                    // if (tutType.HasValue && tutType.Value == ETutorialType.Rotation)
                    //     return;
                    Indicator.Animator.enabled = true;
                    Indicator.Shape.enabled = true;
                    Indicator.Shape.Color = ColorProvider.GetColor(ColorIds.UI).SetA(0f);
                    Indicator.Animator.SetTrigger(AnimKeys.Anim);
                }
                    break;
                case ELevelStage.StartedOrContinued when
                    _Args.PreviousStage != ELevelStage.CharacterKilled
                    && RmazorUtils.MazeContainsGravityItems(Model.GetAllProceedInfos()):
                {
                    // var tutType = Tutorial.IsCurrentLevelTutorial(out _);
                    // if (tutType.HasValue && tutType.Value == ETutorialType.Rotation)
                    //     return;
                    Indicator.Animator.SetTrigger(AnimKeys.Stop);
                }
                    break;
                case ELevelStage.ReadyToUnloadLevel when _Args.PreviousStage != ELevelStage.Paused:
                    Indicator.Shape.enabled = false;
                    break;
            }
        }

        public List<Component> GetRenderers()
        {
            return new List<Component>();
        }
        
        public void OnTutorialStarted(ETutorialType _Type)
        {
            // if (_Type != ETutorialType.Rotation) 
            //     return;
            // Indicator.Animator.enabled = false;
            // Indicator.Shape.enabled = false;
        }

        public void OnTutorialFinished(ETutorialType _Type) { }

        #endregion
    }
}