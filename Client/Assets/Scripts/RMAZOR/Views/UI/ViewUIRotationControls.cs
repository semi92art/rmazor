using System.Collections.Generic;
using Common.Constants;
using Common.Extensions;
using Common.Utils;
using Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using RMAZOR.Views.ContainerGetters;
using RMAZOR.Views.Factories;
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
        #region nonpublic members
        
        private float                         m_BottomOffset;
        private IRotatingPossibilityIndicator m_RotatingPossibilityIndicator;

        #endregion

        #region inject

        private IModelGame                           Model                  { get; }
        private IColorProvider                       ColorProvider          { get; }
        private IContainersGetter                    ContainersGetter       { get; }
        private IManagersGetter                      Managers               { get; }
        private IRotatingPossibilityIndicatorFactory RotatingPossIndFactory { get; }

        public ViewUIRotationControls(
            IModelGame _Model,
            IColorProvider _ColorProvider,
            IContainersGetter _ContainersGetter,
            IManagersGetter _Managers,
            IRotatingPossibilityIndicatorFactory _RotatingPossIndFactory)
        {
            Model = _Model;
            ColorProvider = _ColorProvider;
            ContainersGetter = _ContainersGetter;
            Managers = _Managers;
            RotatingPossIndFactory = _RotatingPossIndFactory;
        }

        #endregion

        #region api
        
        public void Init(Vector4 _Offsets)
        {
            m_BottomOffset = _Offsets.z;
            InitRotatingPossibilityIndicator();
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            m_RotatingPossibilityIndicator.OnLevelStageChanged(_Args);
            switch (_Args.Stage)
            {
                case ELevelStage.ReadyToStart when
                    _Args.PreviousStage != ELevelStage.Paused
                    && RazorMazeUtils.MazeContainsGravityItems(Model.GetAllProceedInfos())
                    && SaveUtils.GetValue(SaveKeys.EnableRotation):
                    m_RotatingPossibilityIndicator.Shape.enabled = true;
                    m_RotatingPossibilityIndicator.Shape.Color = ColorProvider.GetColor(ColorIds.UI).SetA(0f);
                    m_RotatingPossibilityIndicator.Animator.SetTrigger(AnimKeys.Anim);
                    break;
                case ELevelStage.StartedOrContinued when 
                    _Args.PreviousStage != ELevelStage.CharacterKilled
                    && RazorMazeUtils.MazeContainsGravityItems(Model.GetAllProceedInfos())
                    && SaveUtils.GetValue(SaveKeys.EnableRotation):
                    m_RotatingPossibilityIndicator.Animator.SetTrigger(AnimKeys.Stop);
                    break;
                case ELevelStage.ReadyToUnloadLevel when _Args.PreviousStage != ELevelStage.Paused:
                    m_RotatingPossibilityIndicator.Shape.enabled = false;
                    break;
            }
        }

        public List<Component> GetRenderers()
        {
            return new List<Component>();
        }
        
        public void OnTutorialStarted(ETutorialType _Type)
        {
            if (_Type == ETutorialType.Rotation)
            {
                m_RotatingPossibilityIndicator.Animator.enabled = false;
                m_RotatingPossibilityIndicator.Shape.enabled = false;
            }
        }

        public void OnTutorialFinished(ETutorialType _Type)
        {
            if (_Type == ETutorialType.Rotation)
            {
                m_RotatingPossibilityIndicator.Animator.enabled = true;
            }
        }

        #endregion

        #region nonpublic methods

        private void InitRotatingPossibilityIndicator()
        {
            m_RotatingPossibilityIndicator = RotatingPossIndFactory.Create();
        }

        #endregion
    }
}