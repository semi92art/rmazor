using System;
using System.Collections.Generic;
using System.Linq;
using Common.Extensions;
using Common.Managers;
using Common.SpawnPools;
using RMAZOR.Models;
using RMAZOR.Views.Coordinate_Converters;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Common.BackgroundIdleItems
{
    public interface IViewMazeBackgroundIdleItem : ISpawnPoolItem, ICloneable, IOnLevelStageChanged
    {
        Vector2   Position { get; set; }
        void      SetParams(float _Scale,  float             _Thickness);
        void      Init(Transform  _Parent, PhysicsMaterial2D _Material);
        void      SetVelocity(Vector2 _Velocity, float _AngularVelocity);
        void      SetColor(Color      _Color);
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
        protected          Collider2D          Coll;
        protected          Rigidbody2D         Rigidbody { get; set; }

        #endregion

        #region inject

        protected IPrefabSetManager          PrefabSetManager    { get; }
        protected ICoordinateConverter CoordinateConverter { get; }

        protected ViewMazeBackgroundIdleItemBase(
            IPrefabSetManager          _PrefabSetManager,
            ICoordinateConverter _CoordinateConverter)
        {
            PrefabSetManager    = _PrefabSetManager;
            CoordinateConverter = _CoordinateConverter;
        }

        #endregion

        #region api

        public bool ActivatedInSpawnPool
        {
            get => Shapes.First().enabled;
            set
            {
                Coll.enabled = value;
                Shapes.ForEach(_S => _S.enabled = value);
            }
        }

        public Vector2 Position
        {
            get => Obj.transform.position;
            set => Obj.transform.SetPosXY(value);
        }

        public abstract          object Clone();
        public          abstract void   SetParams(float _Scale,  float             _Thickness);
        public abstract          void   Init(Transform  _Parent, PhysicsMaterial2D _Material);
        public abstract          void   SetColor(Color  _Color);

        public void SetVelocity(Vector2 _Velocity, float _AngularVelocity)
        {
            Rigidbody.velocity = _Velocity;
            Rigidbody.angularVelocity = _AngularVelocity;
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.LevelStage != ELevelStage.Loaded)
                return;
            Obj.transform.SetLocalScaleXY(CoordinateConverter.Scale * Vector2.one);
        }

        #endregion
    }
}