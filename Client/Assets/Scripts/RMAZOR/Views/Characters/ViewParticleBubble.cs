using System.Collections.Generic;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Ticker;
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

        protected override IEnumerable<ShapeRenderer> MainShapes   => new[] {m_InnerDisc};
        protected override IEnumerable<ShapeRenderer> BorderShapes => new[] {m_OuterDisc};

        #endregion

        #region inject

        private ViewParticleBubble(
            IContainersGetter    _ContainersGetter,
            ICoordinateConverter _CoordinateConverter,
            IViewGameTicker      _Ticker) 
            : base(
                _ContainersGetter,
                _CoordinateConverter, 
                _Ticker) { }

        #endregion

        #region api

        public override object Clone()
        {
            return new ViewParticleBubble(ContainersGetter, CoordinateConverter, Ticker);
        }

        #endregion

        #region nonpublic methods

        protected override void InitShape()
        {
            var cont = ContainersGetter.GetContainer(ContainerNamesCommon.Background);
            var obj = new GameObject("Bubble Particle");
            obj.SetParent(cont);
            m_InnerDisc = obj.AddComponentOnNewChild<Disc>("Inner Disc", out _)
                .SetType(DiscType.Disc)
                .SetRadius(Radius);
            m_InnerDisc.enabled = false;
            m_OuterDisc = obj.AddComponentOnNewChild<Disc>("Outer Disc", out _)
                .SetType(DiscType.Ring)
                .SetRadius(Radius)
                .SetThickness(BoardThickness);
            m_OuterDisc.enabled = false;
            Transform = obj.transform;
            Rb = obj.AddComponent<Rigidbody2D>();
            Rb.mass = 0f;
            Rb.gravityScale = 0f;
        }

        #endregion
    }
}