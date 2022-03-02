using System;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Exceptions;
using Common.Extensions;
using Common.Managers;
using Common.Providers;
using RMAZOR.Models;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Views.Common.ViewMazeMoneyItems
{
    public interface IViewMazeMoneyItem : IOnLevelStageChanged, ICloneable
    {
        event UnityAction Collected;
        bool              IsCollected { get; set; }
        bool              Active      { get; set; }
        void              Collect(bool   _Collect);
        void              Init(Transform _Parent);
        void              UpdateShape();
        GameObject        Object    { get; }
        ShapeRenderer[]   Renderers { get; }
    }

    public class ViewMazeMoneyItemHexagon : IViewMazeMoneyItem
    {
        #region nonpublic members

        private static AudioClipArgs AudioClipArgsCollectMoneyItem => 
            new AudioClipArgs("collect_point", EAudioClipType.GameSound);
        
        private RegularPolygon m_Renderer1;
        private Disc           m_Renderer2;
        private Animator       m_MoneyItemAnimator;
        private bool           m_Active;

        #endregion

        #region inject

        private ViewSettings             ViewSettings        { get; }
        private IPrefabSetManager        PrefabSetManager    { get; }
        private IColorProvider           ColorProvider       { get; }
        private IMazeCoordinateConverter CoordinateConverter { get; }
        private IAudioManager            AudioManager        { get; }

        public ViewMazeMoneyItemHexagon(
            ViewSettings             _ViewSettings,
            IPrefabSetManager        _PrefabSetManager,
            IColorProvider           _ColorProvider,
            IMazeCoordinateConverter _CoordinateConverter,
            IAudioManager            _AudioManager)
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
            new ViewMazeMoneyItemHexagon(
                ViewSettings, 
                PrefabSetManager,
                ColorProvider, 
                CoordinateConverter, 
                AudioManager);

        public void UpdateShape()
        {
            const float commonScaleCoeff = 0.3f;
            if (m_Renderer1.IsNull() || m_Renderer2.IsNull())
                return;
            float scale = CoordinateConverter.Scale;
            m_Renderer1.Radius = scale * commonScaleCoeff;
            m_Renderer1.Thickness = scale * ViewSettings.LineWidth;
            m_Renderer2.Radius = scale * 0.3f * commonScaleCoeff;
        }

        public GameObject      Object    { get; private set; }
        public ShapeRenderer[] Renderers => new ShapeRenderer[] {m_Renderer1, m_Renderer2};

        public event UnityAction Collected;
        public bool              IsCollected { get; set; }

        public bool Active
        {
            get => m_Active;
            set
            {
                m_Active = value;
                if (m_Renderer1.IsNull() || m_Renderer2.IsNull())
                    return;
                m_Renderer1.enabled = m_Renderer2.enabled = value;
            }
        }

        public void Collect(bool _Collect)
        {
            if (!_Collect || !Active)
                return;
            Dbg.Log(nameof(Collect) + ": " + _Collect);
            Collected?.Invoke();
            AudioManager.PlayClip(AudioClipArgsCollectMoneyItem);
            Active = false;
        }

        public void Init(Transform _Parent)
        {
            ColorProvider.ColorChanged += OnColorChanged;
            InitShape(_Parent);
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (!Active)
                return;
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    m_MoneyItemAnimator.ResetTrigger(AnimKeys.Anim);
                    m_MoneyItemAnimator.SetTrigger(AnimKeys.Anim);
                    break;
                case ELevelStage.Paused:
                    m_MoneyItemAnimator.speed = 0f;
                    break;
                case ELevelStage.ReadyToStart:
                case ELevelStage.StartedOrContinued:
                case ELevelStage.Finished:
                    m_MoneyItemAnimator.speed = 1f;
                    break;
                case ELevelStage.ReadyToUnloadLevel:
                case ELevelStage.Unloaded:
                case ELevelStage.CharacterKilled:
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Args.Stage);
            }
        }

        #endregion

        #region nonpublic methods

        private void InitShape(Transform _Parent)
        {
            Object = PrefabSetManager.InitPrefab(
                _Parent, "views", "money_item");
            m_Renderer1 = Object.GetCompItem<RegularPolygon>("renderer_1");
            m_Renderer2 = Object.GetCompItem<Disc>("renderer_2");
            m_MoneyItemAnimator = Object.GetCompItem<Animator>("animator");
            var col = ColorProvider.GetColor(ColorIds.MoneyItem);
            m_Renderer1.Color = m_Renderer2.Color = col;
            m_Renderer1.SortingOrder = m_Renderer2.SortingOrder = SortingOrders.MoneyItem;
            Object.transform.SetLocalPosXY(Vector2.zero);
            UpdateShape();
        }
        
        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.MoneyItem || !Active || IsCollected)
                return;
            m_Renderer1.Color = m_Renderer2.Color = _Color;
        }

        #endregion
    }
}