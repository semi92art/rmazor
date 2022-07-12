using System.Collections.Generic;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Views.Coordinate_Converters;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Characters
{
    public interface IViewParticleBubble : IViewParticle { }
    
    public class ViewParticleBubble : ViewParticleBase, IViewParticleBubble
    {
        #region constants

        private const float Radius         = 0.1f;
        private const float BoardThickness = 0.03f;

        #endregion
        
        #region nonpublic members
        
        private bool        m_Activated;
        private Disc        m_InnerDisc;
        private Disc        m_OuterDisc;
        private Transform   m_Transform;
        private Rigidbody2D m_Rb;

        protected override IEnumerable<ShapeRenderer> MainShapes   => new[] {m_InnerDisc};
        protected override IEnumerable<ShapeRenderer> BorderShapes => new[] {m_OuterDisc};

        #endregion

        #region inject

        private IContainersGetter          ContainersGetter    { get; }
        private ICoordinateConverter CoordinateConverter { get; }

        private ViewParticleBubble(
            IContainersGetter          _ContainersGetter,
            ICoordinateConverter _CoordinateConverter,
            IViewGameTicker            _Ticker) 
            : base(_Ticker)
        {
            ContainersGetter    = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
        }

        #endregion

        #region api

        public override void Throw(
            Vector2 _Position,
            Vector2 _Speed,
            float   _Scale,
            float   _ThrowTime)
        {
            m_Transform.position = _Position;
            m_Transform.SetLocalScaleXY(Vector2.one * _Scale);
            m_Rb.velocity = _Speed;
            ActivatedInSpawnPool = true;
            Cor.Run(SetColorsOnThrowCoroutine(_ThrowTime)
                .ContinueWith(() => ActivatedInSpawnPool = false));
        }

        public override object Clone()
        {
            return new ViewParticleBubble(ContainersGetter, CoordinateConverter, Ticker);
        }

        #endregion

        #region nonpublic methods

        protected override void InitShape()
        {
            var cont = ContainersGetter.GetContainer(ContainerNames.Background);
            var obj = new GameObject("Bubble Particle");
            obj.SetParent(cont);
            float scale = CoordinateConverter.Scale;
            m_InnerDisc = obj.AddComponentOnNewChild<Disc>("Inner Disc", out _)
                .SetType(DiscType.Disc)
                .SetRadius(scale * Radius);
            m_InnerDisc.enabled = false;
            m_OuterDisc = obj.AddComponentOnNewChild<Disc>("Outer Disc", out _)
                .SetType(DiscType.Ring)
                .SetRadius(scale * Radius)
                .SetThickness(scale * BoardThickness);
            m_OuterDisc.enabled = false;
            m_Transform = obj.transform;
            m_Rb = obj.AddComponent<Rigidbody2D>();
            m_Rb.mass = 0f;
            m_Rb.gravityScale = 0f;
        }

        #endregion
    }
}