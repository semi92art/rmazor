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
    public interface IViewParticleSpark : IViewParticle { }
    
    public class ViewParticleSpark : ViewParticleBase, IViewParticleSpark
    {
        #region serialized fields

        private bool m_Activated;
        private Line m_InnerLine;
        private Line m_OuterLine;

        #endregion

        #region nonpublic members
        
        protected override IEnumerable<ShapeRenderer> MainShapes   => new[] {m_InnerLine};
        protected override IEnumerable<ShapeRenderer> BorderShapes => new[] {m_OuterLine};

        #endregion

        #region inject
        
        private ViewParticleSpark(
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
            return new ViewParticleSpark(ContainersGetter, CoordinateConverter, Ticker);
        }

        public override void Throw(
            Vector2 _Position,
            Vector2 _Speed,
            float   _Scale,
            float   _ThrowTime)
        {
            base.Throw(_Position, _Speed, _Scale, _ThrowTime);
        }

        #endregion

        #region nonpublic methods
        
        protected override void InitShape()
        {
            var cont = ContainersGetter.GetContainer(ContainerNamesCommon.Background);
            var obj = new GameObject("Spark Particle");
            obj.SetParent(cont);
            m_InnerLine = obj.AddComponentOnNewChild<Line>("Inner Line", out _)
                .SetEndCaps(LineEndCap.Round);
            m_InnerLine.enabled = false;
            m_OuterLine = obj.AddComponentOnNewChild<Line>("Outer Line", out _)
                .SetEndCaps(LineEndCap.Round);
            m_OuterLine.enabled = false;
            Transform = obj.transform;
            Rb = obj.AddComponent<Rigidbody2D>();
            Rb.mass = 0f;
            Rb.gravityScale = 0f;
        }

        #endregion
    }
}