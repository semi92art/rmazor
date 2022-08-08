using System;
using Common;
using Common.Exceptions;
using Common.Helpers;
using Common.SpawnPools;
using UnityEngine;

namespace RMAZOR.Views.Characters
{
    public enum EParticleType
    {
        Bubbles,
        Sparks
    }
    
    public interface IViewParticlesThrower : IInit, ICloneable
    {
        EParticleType ParticleType { get; set; }
        void          SetPoolSize(int       _PoolSize);
        void          SetColors(Color       _MainColor, Color _BorderColor);
        void          SetSortingOrder(int   _SortingOrder);
        void          ThrowParticle(Vector2 _StartPosition, Vector2 _Speed, float _Scale, float _ThrowTime);
    }
    
    public class ViewParticlesThrower : InitBase, IViewParticlesThrower
    {
        #region constants

        private const int DefaultPoolSize = 10;

        #endregion
        
        #region nonpublic members

        private int   m_PoolSize = DefaultPoolSize;
        private float m_ThrowTime;
        
        private readonly SpawnPool<IViewParticleBubble> m_PoolBubbles =
            new SpawnPool<IViewParticleBubble>();
        private readonly SpawnPool<IViewParticleSpark> m_PoolSparks =
            new SpawnPool<IViewParticleSpark>();

        #endregion

        #region inject
        
        private IViewParticleBubble ParticleBubble { get; }
        private IViewParticleSpark  ParticleSpark  { get; }

        private ViewParticlesThrower(
            IViewParticleBubble _ParticleBubble,
            IViewParticleSpark  _ParticleSpark)
        {
            ParticleBubble = _ParticleBubble;
            ParticleSpark  = _ParticleSpark;
        }

        #endregion

        #region api
        
        public override void Init()
        {
            InitPools();
            base.Init();
        }
        
        public object Clone() => new ViewParticlesThrower(ParticleBubble, ParticleSpark);

        public EParticleType ParticleType { get; set; }

        public void SetPoolSize(int _PoolSize)
        {
            m_PoolSize = _PoolSize;
        }

        public void SetColors(Color _MainColor, Color _BorderColor)
        {
            foreach (var particle in m_PoolBubbles)
                particle.SetColors(_MainColor, _BorderColor);
        }

        public void SetSortingOrder(int _SortingOrder)
        {
            foreach (var particle in m_PoolBubbles)
                particle.SetSortingOrder(_SortingOrder);
        }

        public void ThrowParticle(
            Vector2 _StartPosition,
            Vector2 _Speed,
            float   _Scale,
            float   _ThrowTime)
        {
            IViewParticle particle;
            switch (ParticleType)
            {
                case EParticleType.Bubbles:
                    particle = m_PoolBubbles.FirstInactive;
                    m_PoolBubbles.Activate((IViewParticleBubble)particle);
                    break;
                case EParticleType.Sparks:
                    particle = m_PoolSparks.FirstInactive;
                    m_PoolSparks.Activate((IViewParticleSpark)particle);
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(ParticleType);
            }
            particle!.Throw(_StartPosition, _Speed, _Scale, _ThrowTime);
        }

        #endregion

        #region nonpublic methods

        private void InitPools()
        {
            for (int i = 0; i < m_PoolSize; i++)
            {
                var bubble = (IViewParticleBubble)ParticleBubble.Clone();
                bubble.Init();
                m_PoolBubbles.Add(bubble);
                var spark = (IViewParticleSpark) ParticleSpark.Clone();
                spark.Init();
                m_PoolSparks.Add(spark);
            }
            m_PoolBubbles.DeactivateAll();
            m_PoolSparks.DeactivateAll();
        }
        
        #endregion
    }
}