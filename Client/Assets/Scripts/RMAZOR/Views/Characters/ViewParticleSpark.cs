using Common.Extensions;
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
            MainShapes = new ShapeRenderer[] {m_InnerLine};
            BorderShapes = new ShapeRenderer[] {m_OuterLine};
        }

        #endregion
    }
}