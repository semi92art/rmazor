using System;
using System.Collections.Generic;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Views.Common.ViewMazeMoneyItems
{
    public interface IViewMazeMoneyItem : IOnLevelStageChanged, ICloneable
    {
        event UnityAction      Collected;
        bool                   IsCollected { get; set; }
        bool                   Active      { get; set; }
        void                   Collect(bool   _Collect);
        void                   Init(Transform _Parent);
        void                   UpdateShape();
        IEnumerable<Component> Renderers { get; }
    }

    public class ViewMazeMoneyItemDisc : InitBase, IViewMazeMoneyItem
    {
        #region nonpublic members

        private static AudioClipArgs AudioClipArgsCollectMoneyItem => 
            new AudioClipArgs("collect_point", EAudioClipType.GameSound);

        private Disc     m_MainDisc;
        private Disc     m_InnerDisc, m_OuterDisc;
        private Animator m_Animator;
        private bool     m_Active;

        #endregion

        #region inject

        private ViewSettings         ViewSettings        { get; }
        private IPrefabSetManager    PrefabSetManager    { get; }
        private IColorProvider       ColorProvider       { get; }
        private ICoordinateConverter CoordinateConverter { get; }
        private IAudioManager        AudioManager        { get; }

        private ViewMazeMoneyItemDisc(
            ViewSettings         _ViewSettings,
            IPrefabSetManager    _PrefabSetManager,
            IColorProvider       _ColorProvider,
            ICoordinateConverter _CoordinateConverter,
            IAudioManager        _AudioManager)
        {
            ViewSettings        = _ViewSettings;
            PrefabSetManager    = _PrefabSetManager;
            ColorProvider       = _ColorProvider;
            CoordinateConverter = _CoordinateConverter;
            AudioManager        = _AudioManager;
        }

        #endregion
        
        #region api
        
        public object Clone() => 
            new ViewMazeMoneyItemDisc(
                ViewSettings, 
                PrefabSetManager,
                ColorProvider, 
                CoordinateConverter, 
                AudioManager);

        public void UpdateShape()
        {
            if (!Initialized)
                return;
            const float commonScaleCoeff = 0.3f;
            float scale = CoordinateConverter.Scale;
            m_MainDisc.transform.SetLocalScaleXY(Vector2.one * scale * commonScaleCoeff);
        }

        public IEnumerable<Component> Renderers => Initialized
            ? new Component[] {m_MainDisc, m_InnerDisc, m_OuterDisc}
            : new Component[] {null};

        public event UnityAction Collected;
        public bool              IsCollected { get; set; }

        public bool Active
        {
            get => m_Active;
            set
            {
                m_Active = value;
                if (!Initialized)
                    return;
                m_MainDisc.enabled = value;
                m_InnerDisc.enabled = value;
                m_OuterDisc.enabled = value;
            }
        }

        public void Collect(bool _Collect)
        {
            if (!_Collect || !Active)
                return;
            Collected?.Invoke();
            AudioManager.PlayClip(AudioClipArgsCollectMoneyItem);
            Active = false;
        }

        public void Init(Transform _Parent)
        {
            if (Initialized)
                return;
            ColorProvider.ColorChanged += OnColorChanged;
            InitShape(_Parent);
            UpdateShape();
            base.Init();
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (!Active)
                return;
            m_Animator.speed = _Args.LevelStage == ELevelStage.Paused ? 0f : 1f;
            switch (_Args.LevelStage)
            {
                case ELevelStage.ReadyToStart:
                    m_Animator.SetTrigger(AnimKeys.Anim);
                    break;
                case ELevelStage.Unloaded:
                    m_Animator.SetTrigger(AnimKeys.Stop);
                    break;
                case ELevelStage.Loaded:
                case ELevelStage.StartedOrContinued:
                case ELevelStage.Paused:
                case ELevelStage.Finished:
                case ELevelStage.ReadyToUnloadLevel:
                case ELevelStage.CharacterKilled:
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Args.LevelStage);
            }
        }

        #endregion

        #region nonpublic methods

        private void InitShape(Transform _Parent)
        {
            var go = PrefabSetManager.InitPrefab(
                _Parent, "views", "money_item");
            m_Animator = go.GetCompItem<Animator>("animator");
            var col = ColorProvider.GetColor(ColorIds.MoneyItem);
            m_MainDisc = go.GetCompItem<Disc>("main_disc")
                .SetSortingOrder(SortingOrders.MoneyItem)
                .SetColorMode(Disc.DiscColorMode.Single)
                .SetColor(col);
            m_OuterDisc = go.GetCompItem<Disc>("outer_disc")
                .SetSortingOrder(SortingOrders.MoneyItem)
                .SetColorMode(Disc.DiscColorMode.Radial);
            m_InnerDisc = go.GetCompItem<Disc>("inner_disc")
                .SetSortingOrder(SortingOrders.MoneyItem)
                .SetColorMode(Disc.DiscColorMode.Radial);
            go.transform.SetLocalPosXY(Vector2.zero);
            Cor.Run(Cor.WaitNextFrame(() =>
            {
                m_OuterDisc.SetColorInner(col)
                    .SetColorOuter(col.SetA(0f));
                m_InnerDisc.SetColorInner(col.SetA(0f))
                    .SetColorOuter(col);
            }));
        }
        
        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.MoneyItem || !Active || IsCollected)
                return;
            m_MainDisc.SetColor(_Color);
            m_OuterDisc.ColorInner = _Color;
            m_OuterDisc.ColorOuter = _Color.SetA(0f);
            m_InnerDisc.ColorInner = _Color.SetA(0f);
            m_InnerDisc.ColorOuter = _Color;
        }

        #endregion
    }
}