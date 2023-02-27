using System;
using System.Collections.Generic;
using Common;
using Common.Constants;
using Common.Extensions;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Characters.Legs
{
    public interface IViewCharacterLegs01 : IViewCharacterLegs { }
    
    public class ViewCharacterLegs01 : InitBase, IViewCharacterLegs01
    {
        #region constants

        private const float RelativeLocalScale = 0.8f;
        
        #endregion
        
        #region nonpublic members

        private Rectangle m_Leg1Body, m_Leg1Border;
        private Rectangle m_Leg2Body, m_Leg2Border;
        private bool      m_Activated;
        
        private EMazeOrientation m_LastMazeOrientation;

        #endregion

        #region inject

        private ViewSettings                ViewSettings        { get; }
        private IContainersGetter           ContainersGetter    { get; }
        private ICoordinateConverter        CoordinateConverter { get; }
        private IPrefabSetManager           PrefabSetManager    { get; }
        private IColorProvider              ColorProvider       { get; }
        private IRendererAppearTransitioner AppearTransitioner  { get; }
        private IModelGame                  Model               { get; }

        private ViewCharacterLegs01(
            ViewSettings                _ViewSettings,
            IContainersGetter           _ContainersGetter,
            ICoordinateConverter        _CoordinateConverter,
            IPrefabSetManager           _PrefabSetManager,
            IColorProvider              _ColorProvider,
            IRendererAppearTransitioner _AppearTransitioner,
            IModelGame                  _Model)
        {
            ViewSettings        = _ViewSettings;
            ContainersGetter    = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
            PrefabSetManager    = _PrefabSetManager;
            ColorProvider       = _ColorProvider;
            AppearTransitioner  = _AppearTransitioner;
            Model               = _Model;
        }

        #endregion

        #region api

        public bool Activated
        {
            get => m_Activated;
            set
            {
                m_Activated = value;
                ActivateShapes(value);
                if (value)
                    UpdatePrefab();
            }
        }
        public EAppearingState  AppearingState { get; private set; }
        
        public void OnRotationFinished(MazeRotationEventArgs _Args)
        {
            m_LastMazeOrientation = _Args.NextOrientation;
        }

        public override void Init()
        {
            if (Initialized)
                return;
            ColorProvider.ColorChanged += OnColorChanged;
            InitPrefab();
            base.Init();
        }

        public void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            ActivateShapes(false);
        }

        public void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args) { }

        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            if (!Model.Character.Alive || Model.PathItemsProceeder.AllPathsProceeded)
                return;
            ActivateShapes(true);
            SetLegsTransform(_Args.Direction);
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.LevelStage)
            {
                case ELevelStage.None:
                    Activated = true;
                    SetLegsTransform(EDirection.Down);
                    return;
                case ELevelStage.Loaded:
                    m_LastMazeOrientation = EMazeOrientation.North;
                    break;
                case ELevelStage.ReadyToStart when 
                    _Args.PreviousStage == ELevelStage.Paused 
                    && _Args.PrePreviousStage == ELevelStage.CharacterKilled:
                {
                    SetLegsTransform(EDirection.Down, true);
                    ActivateShapes(true);
                }
                    break;
                case ELevelStage.CharacterKilled:
                    ActivateShapes(false);
                    break;
            }
        }

        public void OnPathCompleted(V2Int _LastPath)
        {
            ActivateShapes(false);
        }
        
        public void Appear(bool _Appear)
        {
            AppearingState = _Appear ? EAppearingState.Appearing : EAppearingState.Dissapearing;
            var charCol = ColorProvider.GetColor(ColorIds.Character);
            var charCol2 = ColorProvider.GetColor(ColorIds.Character2);
            AppearTransitioner.DoAppearTransition(
                _Appear,
                new Dictionary<IEnumerable<Component>, Func<Color>>
                {
                    {new Component[] {m_Leg1Body, m_Leg2Body}, () => charCol},
                    {new Component[] {m_Leg1Border, m_Leg2Border}, () => charCol2},
                },
                ViewSettings.betweenLevelTransitionTime,
                () =>
                {
                    AppearingState = _Appear ? EAppearingState.Appeared : EAppearingState.Dissapeared;
                });
        }
        
        #endregion

        #region nonpublic methods

        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (!Initialized)
                return;
            switch (_ColorId)
            {
                case ColorIds.Character:  m_Leg1Body.Color   = m_Leg2Body.Color   = _Color; break;
                case ColorIds.Character2: m_Leg1Border.Color = m_Leg2Border.Color = _Color; break;
            }
        }
        
        private void InitPrefab()
        {
            var contGo = ContainersGetter.GetContainer(ContainerNamesMazor.Character).gameObject;
            var go = PrefabSetManager.InitPrefab(
                contGo.transform, 
                CommonPrefabSetNames.Views,
                "character_legs");
            go.transform.SetLocalPosXY(Vector2.zero);
            var bodyCol = ColorProvider.GetColor(ColorIds.Character);
            var borderCol = ColorProvider.GetColor(ColorIds.Character2);
            m_Leg1Body = go
                .GetCompItem<Rectangle>("leg_1_body")
                .SetColor(bodyCol)
                .SetSortingOrder(SortingOrders.Character - 3);
            m_Leg2Body = go
                .GetCompItem<Rectangle>("leg_2_body")
                .SetColor(bodyCol)
                .SetSortingOrder(SortingOrders.Character - 3);
            m_Leg1Border = go.GetCompItem<Rectangle>("leg_1_border")
                .SetColor(borderCol)
                .SetSortingOrder(SortingOrders.Character - 2);
            m_Leg2Border = go.GetCompItem<Rectangle>("leg_2_border")
                .SetColor(borderCol)
                .SetSortingOrder(SortingOrders.Character - 2);
            Activated = false;
        }

        private void UpdatePrefab()
        {
            float scale = CoordinateConverter.Scale;
            if (MathUtils.Equals(scale, 0f))
                scale = 1f;
            var localScale = Vector2.one * scale * RelativeLocalScale;
            m_Leg1Body.transform.SetLocalScaleXY(localScale);
            m_Leg2Body.transform.SetLocalScaleXY(localScale);
            SetLegsTransform(EDirection.Down);
        }
        
        private void SetLegsTransform(EDirection _Direction, bool _ByLastOrientation = false)
        {
            float scale = CoordinateConverter.Scale;
            if (MathUtils.Equals(scale, 0f))
                scale = 1f;
            Vector2 dir = RmazorUtils.GetDirectionVector(_Direction, Model.MazeRotation.Orientation);
            var a = dir * 0.39f;
            var dirOrth = new Vector2(dir.y, dir.x);
            var b = a + dirOrth * 0.25f;
            var c = a - dirOrth * 0.25f;
            m_Leg1Body.transform.localPosition = b * scale;
            m_Leg2Body.transform.localPosition = c * scale;
            var orientationForAngle = _ByLastOrientation ? m_LastMazeOrientation : Model.MazeRotation.Orientation;
            bool orth = (orientationForAngle == EMazeOrientation.East || orientationForAngle == EMazeOrientation.West) 
                        && _ByLastOrientation;
            float legsAngle = _Direction switch
            {
                EDirection.Down  => !orth ? 0f   : 90f,
                EDirection.Right => !orth ? 90f  : 0f,
                EDirection.Up    => !orth ? 180f : 90f,
                EDirection.Left  => !orth ? 270f : 0f,
                _                        => default
            };
            var rotation =  Quaternion.Euler(Vector3.forward * legsAngle);
            m_Leg1Body.transform.rotation = rotation;
            m_Leg2Body.transform.rotation = rotation;
        }

        private void ActivateShapes(bool _Active)
        {
            m_Leg1Body.enabled = _Active;
            m_Leg2Body.enabled = _Active;
            m_Leg1Border.enabled = _Active;
            m_Leg2Border.enabled = _Active;
        }

        #endregion
    }
}