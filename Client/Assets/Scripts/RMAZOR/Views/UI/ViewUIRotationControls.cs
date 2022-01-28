using System.Collections.Generic;
using Common.Constants;
using Common.Extensions;
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

        private IModelGame                           Model                  { get; }
        private IColorProvider                       ColorProvider          { get; }
        private IRotatingPossibilityIndicator        Indicator              { get; }

        public ViewUIRotationControls(
            IModelGame                    _Model,
            IColorProvider                _ColorProvider,
            IRotatingPossibilityIndicator _Indicator)
        {
            Model         = _Model;
            ColorProvider = _ColorProvider;
            Indicator     = _Indicator;
        }

        #endregion

        #region api
        
        public void Init(Vector4 _Offsets)
        {
            Indicator.Name = "Rotating Indicator";
            Indicator.Init(_Offsets);
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            Indicator.OnLevelStageChanged(_Args);
            switch (_Args.Stage)
            {
                case ELevelStage.ReadyToStart when
                    _Args.PreviousStage != ELevelStage.Paused
                    && RazorMazeUtils.MazeContainsGravityItems(Model.GetAllProceedInfos())
                    && SaveUtils.GetValue(SaveKeys.EnableRotation):
                    Indicator.Shape.enabled = true;
                    Indicator.Shape.Color = ColorProvider.GetColor(ColorIds.UI).SetA(0f);
                    Indicator.Animator.SetTrigger(AnimKeys.Anim);
                    break;
                case ELevelStage.StartedOrContinued when 
                    _Args.PreviousStage != ELevelStage.CharacterKilled
                    && RazorMazeUtils.MazeContainsGravityItems(Model.GetAllProceedInfos())
                    && SaveUtils.GetValue(SaveKeys.EnableRotation):
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
            Indicator.Animator.enabled = true;
        }

        #endregion

        #region nonpublic methods

        // private void InitRotatingPossibilityIndicator()
        // {
        //     m_RotatingPossibilityIndicator = RotatingPossIndFactory.Create();
        // }

        #endregion
    }
}