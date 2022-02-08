using System.Collections.Generic;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Extensions;
using Common.Providers;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using UnityEngine;

namespace RMAZOR.Views.UI
{
    public interface IViewUIRotationControls :
        IOnLevelStageChanged,
        IInitViewUIItem,
        IViewUIGetRenderers
    {
        void OnTutorialStarted(ETutorialType _Type);
        void OnTutorialFinished(ETutorialType _Type);
    }
    
    public class ViewUIRotationControls : IViewUIRotationControls
    {
        #region inject

        private IModelGame                    Model          { get; }
        private IColorProvider                ColorProvider  { get; }
        private IRotatingPossibilityIndicator Indicator      { get; }
        private ICameraProvider               CameraProvider { get; }

        public ViewUIRotationControls(
            IModelGame                    _Model,
            IColorProvider                _ColorProvider,
            IRotatingPossibilityIndicator _Indicator,
            ICameraProvider               _CameraProvider)
        {
            Model         = _Model;
            ColorProvider = _ColorProvider;
            Indicator     = _Indicator;
            CameraProvider = _CameraProvider;
        }

        #endregion

        #region api
        
        public void Init(Vector4 _Offsets)
        {
            Indicator.Name = "Rotating Indicator";
            Indicator.Init(_Offsets);
            var bounds = GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera);
            Indicator.SetPosition(new Vector2(bounds.center.x, bounds.min.y + 8f));
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            Indicator.OnLevelStageChanged(_Args);
            switch (_Args.Stage)
            {
                case ELevelStage.ReadyToStart when
                    _Args.PreviousStage != ELevelStage.Paused
                    && RazorMazeUtils.MazeContainsGravityItems(Model.GetAllProceedInfos())
                    && SaveUtils.GetValue(SaveKeysRmazor.EnableRotation):
                    Indicator.Shape.enabled = true;
                    Indicator.Shape.Color = ColorProvider.GetColor(ColorIdsCommon.UI).SetA(0f);
                    Indicator.Animator.SetTrigger(AnimKeys.Anim);
                    break;
                case ELevelStage.StartedOrContinued when 
                    _Args.PreviousStage != ELevelStage.CharacterKilled
                    && RazorMazeUtils.MazeContainsGravityItems(Model.GetAllProceedInfos())
                    && SaveUtils.GetValue(SaveKeysRmazor.EnableRotation):
                    Indicator.Animator.SetTrigger(AnimKeys.Stop);
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
            if (_Type != ETutorialType.Rotation) 
                return;
            Indicator.Animator.enabled = false;
            Indicator.Shape.enabled = false;
        }

        public void OnTutorialFinished(ETutorialType _Type)
        {
            if (_Type != ETutorialType.Rotation)
                return;
            Cor.Run(Cor.WaitWhile(
                () => Model.LevelStaging.LevelStage != ELevelStage.Loaded,
                () => Indicator.Animator.enabled = true));
        }

        #endregion
    }
}