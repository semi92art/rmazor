using System;
using System.Collections;
using Common;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.SpawnPools;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Characters
{
    public interface IViewBubbleItem : ISpawnPoolItem, ICloneable, IInit
    {
        void Throw(Vector2 _Position, Vector2 _Speed, float _Scale);
    }
    
    public class ViewBubbleItem : InitBase, IViewBubbleItem
    {
        #region constants

        private const float Radius         = 0.1f;
        private const float BoardThickness = 0.03f;

        #endregion
        
        #region nonpublic members
        
        private bool        m_Activated;
        private Disc        m_InnerDisc;
        private Disc        m_OuterDisc;
        private GameObject  m_Obj;
        private Transform   m_CharacterContainer, m_BackgroundContainer;
        private Rigidbody2D m_Rb;

        #endregion

        #region inject
        
        private IColorProvider           ColorProvider       { get; }
        private IContainersGetter        ContainersGetter    { get; }
        private IMazeCoordinateConverter CoordinateConverter { get; }
        private IViewGameTicker          Ticker              { get; }

        public ViewBubbleItem(
            IColorProvider           _ColorProvider,
            IContainersGetter        _ContainersGetter,
            IMazeCoordinateConverter _CoordinateConverter,
            IViewGameTicker          _Ticker)
        {
            ColorProvider       = _ColorProvider;
            ContainersGetter    = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
            Ticker              = _Ticker;
        }

        #endregion

        #region api
        
        public bool ActivatedInSpawnPool
        {
            get => m_Activated;
            set
            {
                if (value && !Initialized)
                    Init();
                m_Activated = value;
                m_InnerDisc.enabled = m_OuterDisc.enabled = value;
            }
        }

        public override void Init()
        {
            ColorProvider.ColorChanged += OnColorChanged;
            InitShape();
            base.Init();
        }

        public void Throw(Vector2 _Position, Vector2 _Speed, float _Scale)
        {
            m_Obj.SetParent(m_BackgroundContainer);
            m_Obj.transform.position = _Position;
            m_Obj.transform.SetLocalScaleXY(Vector2.one * _Scale);
            m_Rb.velocity = _Speed;
            ActivatedInSpawnPool = true;
            Cor.Run(ThrowCoroutine());
        }
        
        public object Clone()
        {
            return new ViewBubbleItem(ColorProvider, ContainersGetter, CoordinateConverter, Ticker);
        }

        #endregion

        #region nonpublic methods
        
        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (!Initialized)
                return;
            switch (_ColorId)
            {
                case ColorIds.Character:  m_InnerDisc.SetColor(_Color); break;
                // case ColorIds.Character2: m_OuterDisc.SetColor(_Color); break;
            }
        }

        private void InitShape()
        {
            m_CharacterContainer = ContainersGetter.GetContainer(ContainerNames.Character);
            m_BackgroundContainer = ContainersGetter.GetContainer(ContainerNames.Background);
            m_Obj = new GameObject("Bubble Tail Item");
            m_Obj.SetParent(m_BackgroundContainer);
            float scale = CoordinateConverter.Scale;
            m_InnerDisc = m_Obj.AddComponentOnNewChild<Disc>("Inner Disc", out _)
                .SetType(DiscType.Disc)
                .SetSortingOrder(SortingOrders.Character - 1)
                .SetRadius(scale * Radius)
                .SetColor(ColorProvider.GetColor(ColorIds.Character));
            m_InnerDisc.enabled = false;
            m_OuterDisc = m_Obj.AddComponentOnNewChild<Disc>("Outer Disc", out _)
                .SetType(DiscType.Ring)
                .SetSortingOrder(SortingOrders.Character - 2)
                .SetRadius(scale * Radius)
                .SetThickness(scale * BoardThickness)
                .SetColor(Color.black);
            m_OuterDisc.enabled = false;
            m_Rb = m_Obj.AddComponent<Rigidbody2D>();
            m_Rb.mass = 0f;
            m_Rb.gravityScale = 0f;
        }

        private IEnumerator ThrowCoroutine()
        {
            yield return Cor.Lerp(
                Ticker,
                0.2f,
                _OnProgress: _P =>
                {
                    m_InnerDisc.SetColor(m_InnerDisc.Color.SetA(_P));
                    m_OuterDisc.SetColor(m_OuterDisc.Color.SetA(_P));
                },
                _OnFinish: () =>
                {
                    ActivatedInSpawnPool = false;
                },
                _ProgressFormula: _P =>
                {
                    _P = 1f - _P;
                    const float threshold = 0.3f;
                    if (_P > threshold)
                        return 1f;
                    return _P / threshold;
                });
        }

        #endregion
    }
}