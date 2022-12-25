using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using Common.Extensions;
using Common.Helpers;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.SpawnPools;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Views.Coordinate_Converters;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Characters
{
    public interface IViewParticle : ISpawnPoolItem, ICloneable, IInit
    {
        void Throw(Vector2       _Position,  Vector2 _Speed, float _Scale, float _ThrowTime);
        void SetColors(Color     _MainColor, Color   _BorderColor);
        void SetSortingOrder(int _SortingOrder);
    }
    
    public abstract class ViewParticleBase : InitBase, IViewParticle
    {
        #region nonpublic members
        
        private bool m_Activated;
        
        protected abstract IEnumerable<ShapeRenderer> MainShapes   { get; }
        protected abstract IEnumerable<ShapeRenderer> BorderShapes { get; }
        
        protected Transform   Transform;
        protected Rigidbody2D Rb;

        #endregion

        #region inject

        protected IContainersGetter    ContainersGetter    { get; }
        protected ICoordinateConverter CoordinateConverter { get; }
        protected IViewGameTicker      Ticker              { get; }
        
        protected ViewParticleBase(
            IContainersGetter    _ContainersGetter,
            ICoordinateConverter _CoordinateConverter,
            IViewGameTicker      _Ticker)
        {
            ContainersGetter    = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
            Ticker              = _Ticker;
        }

        #endregion

        #region api
        
        public virtual bool ActivatedInSpawnPool
        {
            get => m_Activated;
            set
            {
                if (value && !Initialized)
                    Init();
                foreach (var shape in MainShapes)
                    shape.enabled = value;
                foreach (var shape in BorderShapes)
                    shape.enabled = value;
                m_Activated = value;
            }
        }
        
        public override void Init()
        {
            InitShape();
            base.Init();
        }
        
        public void SetColors(Color _MainColor, Color _BorderColor)
        {
            if (Initialized)
            {
                SetColorsCore(_MainColor, _BorderColor);
            }
            else
            {
                Cor.Run(Cor.WaitWhile(() => !Initialized,
                    () =>
                    {
                        SetColorsCore(_MainColor, _BorderColor);
                    }));
            }
        }
        
        public void SetSortingOrder(int _SortingOrder)
        {
            foreach (var shape in MainShapes)
                shape.SetSortingOrder(_SortingOrder);
            foreach (var shape in BorderShapes)
                shape.SetSortingOrder(_SortingOrder - 1);
        }

        public abstract object Clone();

        public virtual void Throw(
            Vector2 _Position,
            Vector2 _Speed,
            float   _Scale,
            float   _ThrowTime)
        {
            Transform.position = _Position;
            Transform.SetLocalScaleXY(Vector2.one * _Scale);
            Rb.velocity = _Speed;
            ActivatedInSpawnPool = true;
            Cor.Run(SetColorsOnThrowCoroutine(_ThrowTime)
                .ContinueWith(() => ActivatedInSpawnPool = false));
        }

        #endregion

        #region nonpublic methods
        
        protected abstract void InitShape();
        
        private void SetColorsCore(Color _MainColor, Color _BorderColor)
        {
            foreach (var shape in MainShapes)
                shape.SetColor(_MainColor);
            foreach (var shape in BorderShapes)
                shape.SetColor(_BorderColor);
        }
        
        protected virtual IEnumerator SetColorsOnThrowCoroutine(float _ThrowTime)
        {
            yield return Cor.Lerp(
                Ticker,
                _ThrowTime,
                _OnProgress: _P =>
                {
                    foreach (var shape in MainShapes)
                        shape.SetColor(shape.Color.SetA(_P));
                    foreach (var shape in BorderShapes)
                        shape.SetColor(shape.Color.SetA(_P));
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