using System;
using System.Collections.Generic;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Exceptions;
using Common.Extensions;
using Common.Managers;
using Common.Providers;
using RMAZOR.Models;
using RMAZOR.Views.Utils;
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

    public class ViewMazeMoneyItemCrypto : IViewMazeMoneyItem
    {
        #region nonpublic members

        private static AudioClipArgs AudioClipArgsCollectMoneyItem => 
            new AudioClipArgs("collect_point", EAudioClipType.GameSound);
        
        private SpriteRenderer m_Renderer;
        private Animator       m_MoneyItemAnimator;
        private bool           m_Active;

        #endregion

        #region inject

        private ViewSettings             ViewSettings        { get; }
        private IPrefabSetManager        PrefabSetManager    { get; }
        private IColorProvider           ColorProvider       { get; }
        private IMazeCoordinateConverter CoordinateConverter { get; }
        private IAudioManager            AudioManager        { get; }

        public ViewMazeMoneyItemCrypto(
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
            new ViewMazeMoneyItemCrypto(
                ViewSettings, 
                PrefabSetManager,
                ColorProvider, 
                CoordinateConverter, 
                AudioManager);

        public void UpdateShape()
        {
            const float commonScaleCoeff = 0.3f;
            if (m_Renderer.IsNull())
                return;
            float scale = CoordinateConverter.Scale;
            m_Renderer.transform.localScale = Vector3.one * scale * commonScaleCoeff;
        }

        public IEnumerable<Component> Renderers => new Component[] {m_Renderer};

        public event UnityAction Collected;
        public bool              IsCollected { get; set; }

        public bool Active
        {
            get => m_Active;
            set
            {
                m_Active = value;
                if (m_Renderer.IsNull())
                    return;
                m_Renderer.enabled = value;
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
            ColorProvider.ColorChanged += OnColorChanged;
            InitShape(_Parent);
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (!Active)
                return;
            m_MoneyItemAnimator.speed = _Args.Stage == ELevelStage.Paused ? 0f : 1f;
            switch (_Args.Stage)
            {
                case ELevelStage.ReadyToStart:
                    m_MoneyItemAnimator.SetTrigger(AnimKeys.Anim);
                    break;
                case ELevelStage.Unloaded:
                    m_MoneyItemAnimator.SetTrigger(AnimKeys.Stop);
                    break;
                case ELevelStage.Loaded:
                case ELevelStage.StartedOrContinued:
                case ELevelStage.Paused:
                case ELevelStage.Finished:
                case ELevelStage.ReadyToUnloadLevel:
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
            var go = PrefabSetManager.InitPrefab(
                _Parent, "views", "money_item");
            m_Renderer = go.GetCompItem<SpriteRenderer>("renderer_1");
            m_MoneyItemAnimator = go.GetCompItem<Animator>("animator");
            var col = ColorProvider.GetColor(ColorIds.MoneyItem);
            m_Renderer.color = col;
            m_Renderer.sortingOrder = SortingOrders.MoneyItem;
            go.transform.SetLocalPosXY(Vector2.zero);
            UpdateShape();
        }
        
        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.MoneyItem || !Active || IsCollected)
                return;
            m_Renderer.color = _Color;
        }

        #endregion
    }
}