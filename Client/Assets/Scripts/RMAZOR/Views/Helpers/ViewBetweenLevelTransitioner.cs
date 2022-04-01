using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models;
using Shapes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace RMAZOR.Views.Helpers
{
    public enum EAppearTransitionType
    {
        Simple,
        WithoutDelay,
        Random
    }
    
    public interface IViewBetweenLevelTransitioner : IInit
    {
        void DoAppearTransition(
            bool _Appear,
            Dictionary<IEnumerable<Component>, Func<Color>> _Sets,
            UnityAction _OnFinish = null,
            EAppearTransitionType _Type = EAppearTransitionType.Simple);
    }

    public class ViewBetweenLevelTransitioner 
        : InitBase,
          IViewBetweenLevelTransitioner,
          IOnLevelStageChanged
    {
        #region nonpublic members

        private MeshRenderer m_BetweenLevelTextureRend;
        private float        m_FullTransitionTime;

        #endregion
        
        #region inject
        
        private IViewGameTicker              GameTicker                  { get; }
        private IBetweenLevelTextureProvider BetweenLevelTextureProvider { get; }
        private ViewSettings                 ViewSettings                { get; }

        public ViewBetweenLevelTransitioner(
            IViewGameTicker              _GameTicker,
            IBetweenLevelTextureProvider _BetweenLevelTextureProvider,
            ViewSettings                 _ViewSettings)
        {
            GameTicker                  = _GameTicker;
            BetweenLevelTextureProvider = _BetweenLevelTextureProvider;
            ViewSettings                = _ViewSettings;
        }

        #endregion

        #region api


        public override void Init()
        {
            BetweenLevelTextureProvider.Init();
            var col1 = CommonData.CompanyLogoBackgroundColor;
            Color.RGBToHSV(col1, out float h, out float s, out float v);
            var col2 = Color.HSVToRGB(h, s + 0.05f, v + 0.05f);
            BetweenLevelTextureProvider.SetColors(col1, col2);
            base.Init();
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    m_FullTransitionTime = GetDelay(EAppearTransitionType.Simple);
                    Cor.Run(DoTextureTransition(false));
                    break;
                case ELevelStage.ReadyToUnloadLevel:
                    Cor.Run(DoTextureTransition(true));
                    break;
            }
        }

        public void DoAppearTransition(
            bool _Appear,
            Dictionary<IEnumerable<Component>, Func<Color>> _Sets,
            UnityAction _OnFinish = null,
            EAppearTransitionType _Type = EAppearTransitionType.Simple)
        {
            float delay = GetDelay(_Type);
            foreach (var set in _Sets)
            {
                var endCol = !_Appear ? () => set.Value().SetA(0f) : set.Value;
                var shapes = set.Key.Where(_Shape => _Shape.IsNotNull()).ToList();
                void FastTransit()
                {
                    foreach (var shape in shapes)
                    {
                        switch (shape)
                        {
                            case Behaviour beh: beh.enabled = true; break;
                            case Renderer rend: rend.enabled = true; break;
                        }
                        switch (shape)
                        {
                            case ShapeRenderer shapeRenderer:   shapeRenderer.Color  = endCol(); break;
                            case SpriteRenderer spriteRenderer: spriteRenderer.color = endCol(); break;
                            case TextMeshPro textMeshPro:       textMeshPro.color    = endCol(); break;
                        }
                    }
                    _OnFinish?.Invoke();
                }
                if (_Appear)
                    FastTransit();
                if (!_Appear)
                    Cor.Run(Cor.Delay(
                        delay,
                        FastTransit));
            }
        }

        #endregion

        #region nonpublic methods

        private float GetDelay(EAppearTransitionType _Type)
        {
            float coeff = ViewSettings.mazeItemTransitionDelayCoefficient;
            switch (_Type)
            {
                case EAppearTransitionType.Simple:
                    return ViewSettings.mazeItemTransitionTime;
                case EAppearTransitionType.Random:
                    return Random.value * 10f * coeff;
                case EAppearTransitionType.WithoutDelay:
                    return 0;
                default:
                    throw new SwitchCaseNotImplementedException(_Type);
            }
        }

        private IEnumerator DoTextureTransition(bool _Appear)
        {
            if (_Appear)
                BetweenLevelTextureProvider.Activate(true);
            yield return Cor.Lerp(
                _Appear ? 0f : 1f,
                _Appear ? 1f : 0f,
                m_FullTransitionTime,
                _P => BetweenLevelTextureProvider.SetTransitionValue(_P),
                GameTicker,
                (_Broken, _Progress) =>
                {
                    if (!_Appear)
                        BetweenLevelTextureProvider.Activate(false);
                },
                _ProgressFormula: _P =>
                {
                    float p = _Appear ? (_P - 0.2f) * 1.25f : _P * 1.25f;
                    p = MathUtils.Clamp(p, 0f, 1f);
                    return p;
                });
        }

        #endregion
    }
}