using System;
using System.Collections.Generic;
using DI.Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeItems.Props;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItems
{
    public enum EAppearingState
    {
        Appearing,
        Appeared,
        Dissapearing,
        Dissapeared
    }

    public enum EProceedingStage
    {
        Inactive,
        Active,
        ActiveAndWorking
    }

    public abstract class ViewMazeItemBase : IViewMazeItem
    {
        #region nonpublic members
        
        protected abstract object[] Shapes { get; }
        protected bool Initialized { get; set; }
        protected bool m_Activated;
        
        #endregion

        #region inject

        protected ViewSettings ViewSettings { get; }
        protected IModelGame Model { get; }
        protected ICoordinateConverter CoordinateConverter { get; }
        protected IContainersGetter ContainersGetter { get; }
        protected IGameTicker GameTicker { get; }
        

        protected ViewMazeItemBase (
            ViewSettings _ViewSettings,
            IModelGame _Model,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTicker _GameTicker)
        {
            ViewSettings = _ViewSettings;
            Model = _Model;
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter = _ContainersGetter;
            GameTicker = _GameTicker;
            
            GameTicker.Register(this);
        }

        #endregion
        
        #region api
        
        public ViewMazeItemProps Props { get; set; }
        public abstract object Clone();
        
        public virtual bool Activated
        {
            get => m_Activated;
            set
            {
                m_Activated = value;
                if (!value)
                    DeactivateShapes();
            }
        }
        
        public virtual GameObject Object { get; protected set; }
        
        public virtual EAppearingState AppearingState { get; set; }
        public virtual EProceedingStage ProceedingStage { get; set; }

        public virtual void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Loaded)
                Appear(true);
            else if (_Args.Stage == ELevelStage.Unloaded)
                Appear(false);
            
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                case ELevelStage.Finished:
                    ProceedingStage = EProceedingStage.Active;
                    break;
                case ELevelStage.ReadyToStartOrContinue:
                case ELevelStage.StartedOrContinued:
                    ProceedingStage = EProceedingStage.ActiveAndWorking;
                    break;
                case ELevelStage.Paused:
                case ELevelStage.Unloaded:
                    ProceedingStage = EProceedingStage.Inactive;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public virtual void Init(ViewMazeItemProps _Props)
        {
            Props = _Props;
            SetShape();
            // GameTicker.Register(this);
            Initialized = true;
        }

        public bool Equal(MazeItem _MazeItem)
        {
            if (Props == null)
                return false;
            return _MazeItem.Path == Props.Path && _MazeItem.Type == Props.Type;
        }
        
        public void SetLocalPosition(Vector2 _Position) => Object.transform.SetLocalPosXY(_Position);
        public void SetLocalScale(float _Scale) => Object.transform.localScale = _Scale * Vector3.one;

        #endregion
        
        #region nonpublic methods
        
        protected abstract void SetShape();

        protected void DeactivateShapes()
        {
            foreach (var shape in Shapes)
            {
                if (shape is ShapeRenderer shapeRenderer && !shapeRenderer.IsNull())
                    shapeRenderer.enabled = false;
                else if (shape is SpriteRenderer spriteRenderer && !spriteRenderer.IsNull())
                    spriteRenderer.enabled = false;
                else if (shape is MeshRenderer meshRenderer && !meshRenderer.IsNull())
                    meshRenderer.enabled = false;
            }
        }
        
        protected virtual void Appear(bool _Appear)
        {
            AppearingState = _Appear ? EAppearingState.Appearing : EAppearingState.Dissapearing;
            Coroutines.Run(Coroutines.WaitWhile(
                () => !Initialized,
                () =>
                {
                    RazorMazeUtils.DoAppearTransitionSimple(
                        _Appear,
                        GameTicker,
                        new Dictionary<object[], Color>
                        {
                            {Shapes, DrawingUtils.ColorLines}
                        },
                        _OnFinish: () =>
                        {
                            if (!_Appear)
                                DeactivateShapes();
                            AppearingState = _Appear ? EAppearingState.Appeared : EAppearingState.Dissapeared;
                        });
                }));
        }

        #endregion
    }
}