using System;
using System.Collections.Generic;
using System.Linq;
using Common.Extensions;
using Common.SpawnPools;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Common.BackgroundIdleItems
{
    public interface IViewMazeBackgroundIdleItem : ISpawnPoolItem, ICloneable
    {
        void    Init(Transform _Parent, PhysicsMaterial2D _Material);
        Vector2 Position { get; set; }
        void    SetVelocity(Vector2 _Velocity);
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

        #region api

        public bool ActivatedInSpawnPool
        {
            get => Shapes.First().enabled;
            set => Shapes.ForEach(_S => _S.enabled = value);
        }
        
        public abstract object Clone();


        public abstract void    Init(Transform _Parent, PhysicsMaterial2D _Material);

        public Vector2 Position
        {
            get => Obj.transform.position;
            set => Obj.transform.SetPosXY(value);
        }

        public void SetVelocity(Vector2 _Velocity)
        {
            Rigidbody.velocity = _Velocity;
        }

        public abstract void SetColor(Color _Color);

        #endregion
    }
}