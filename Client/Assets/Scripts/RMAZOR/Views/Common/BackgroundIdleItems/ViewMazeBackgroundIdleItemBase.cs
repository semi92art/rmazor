using System;
using System.Collections.Generic;
using System.Linq;
using Common.Extensions;
using Common.Managers;
using Common.SpawnPools;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Common.BackgroundIdleItems
{
    public interface IViewMazeBackgroundIdleItem : ISpawnPoolItem, ICloneable
    {
        void    SetParams(float _Scale,  float             _Thickness);
        void    Init(Transform  _Parent, PhysicsMaterial2D _Material);
        Vector2 Position { get; set; }
        void    SetVelocity(Vector2 _Velocity, float _AngularVelocity);
        void    SetColor(Color      _Color);
    }
    
    public abstract class ViewMazeBackgroundIdleItemBase : IViewMazeBackgroundIdleItem
    {

        #region nonpublic members

        private GameObject m_Obj;
        protected GameObject Obj
        {
            get => m_Obj;
            set
            {
                m_Obj = value;
                m_Obj.layer = LayerMask.NameToLayer("ω Omega"); 
            }
        }
        
        protected readonly List<ShapeRenderer> Shapes = new List<ShapeRenderer>();
        protected          Rigidbody2D         Rigidbody { get; set; }

        #endregion

        #region inject
        
        protected IPrefabSetManager PrefabSetManager { get; }

        protected ViewMazeBackgroundIdleItemBase(IPrefabSetManager _PrefabSetManager)
        {
            PrefabSetManager = _PrefabSetManager;
        }

        #endregion

        #region api

        public bool ActivatedInSpawnPool
        {
            get => Shapes.First().enabled;
            set => Shapes.ForEach(_S => _S.enabled = value);
        }

        public abstract          object Clone();
        public          abstract void   SetParams(float _Scale, float _Thickness);


        public abstract void    Init(Transform _Parent, PhysicsMaterial2D _Material);

        public Vector2 Position
        {
            get => Obj.transform.position;
            set => Obj.transform.SetPosXY(value);
        }

        public void SetVelocity(Vector2 _Velocity, float _AngularVelocity)
        {
            Rigidbody.velocity = _Velocity;
            Rigidbody.angularVelocity = _AngularVelocity;
        }

        public abstract void SetColor(Color _Color);

        #endregion
    }
}