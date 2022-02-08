using Common;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using RMAZOR.Managers;
using RMAZOR.Models;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.UI
{
    public interface IRotatingPossibilityIndicator : IOnLevelStageChanged, IInitViewUIItem
    {
        string             Name      { set; }
        Animator           Animator  { get; }
        Rectangle          Shape     { get; }
        AnimationTriggerer Triggerer { get; }
        void               SetPosition(Vector2 _Position);
    }
    
    public class RotatingPossibilityIndicator : IRotatingPossibilityIndicator
    {
        #region nonpublic members

        private Transform m_Transform;

        #endregion
        
        #region inject

        private IContainersGetter ContainersGetter { get; }
        private IManagersGetter   Managers         { get; }
        private IColorProvider    ColorProvider    { get; }

        public RotatingPossibilityIndicator(
            IContainersGetter _ContainersGetter,
            IManagersGetter   _Managers,
            IColorProvider    _ColorProvider)
        {
            ContainersGetter = _ContainersGetter;
            Managers         = _Managers;
            ColorProvider    = _ColorProvider;
        }

        #endregion

        #region api

        public string             Name      { get; set; }
        public Animator           Animator  { get; private set; }
        public Rectangle          Shape     { get; private set; }
        public AnimationTriggerer Triggerer { get; private set; }
        
        public void SetPosition(Vector2 _Position)
        {
            m_Transform.SetPosXY(_Position);
        }

        public void Init(Vector4 _Offsets)
        {
            InitShape();
            ColorProvider.ColorChanged += OnColorChanged;
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            Animator.speed = _Args.Stage == ELevelStage.Paused ? 0f : 1f;
        }

        #endregion

        #region nonpublic methods
        
        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIdsCommon.UI)
                return;
            Shape.Color = _Color;
        }

        private void InitShape()
        {
            const float scale = 3f;
            var cont = ContainersGetter.GetContainer(ContainerNames.GameUI);
            var goIndicator = Managers.PrefabSetManager.InitPrefab(
                cont, "ui_game", "rotating_possibility_indicator");
            goIndicator.name = Name;
            Shape     = goIndicator.GetCompItem<Rectangle>("indicator");
            Animator  = goIndicator.GetCompItem<Animator>("animator");
            Triggerer = goIndicator.GetCompItem<AnimationTriggerer>("triggerer");
            m_Transform = goIndicator.transform;
            m_Transform.localScale = Vector3.one * scale;

        }

        #endregion
    }
}