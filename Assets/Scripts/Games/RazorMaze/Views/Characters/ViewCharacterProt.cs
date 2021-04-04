using System.Linq;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeCommon;
using Games.RazorMaze.Views.MazeItems;
using Shapes;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.Characters
{
    public class ViewCharacterProt : IViewCharacter
    {
        #region nonpublic members
        
        private Vector2 m_PrevPos;
        private Vector2 m_NextPos;
        private Disc m_Shape;
        
        #endregion
     
        #region inject
        
        private ICoordinateConverter CoordinateConverter { get; }
        private IModelMazeData ModelMazeData { get; }
        private IContainersGetter ContainersGetter { get; }
        private IViewMazeCommon ViewMazeCommon { get; }

        public ViewCharacterProt(
            ICoordinateConverter _CoordinateConverter, 
            IModelMazeData _ModelMazeData, 
            IContainersGetter _ContainersGetter,
            IViewMazeCommon _ViewMazeCommon)
        {
            CoordinateConverter = _CoordinateConverter;
            ModelMazeData = _ModelMazeData;
            ContainersGetter = _ContainersGetter;
            ViewMazeCommon = _ViewMazeCommon;
        }
        
        #endregion
        
        #region api
        
        public void Init()
        {
            CoordinateConverter.Init(ModelMazeData.Info.Size);
            InitShape(0.4f * CoordinateConverter.GetScale(), new Color(1f, 0.38f, 0f));
            var pos = CoordinateConverter.ToLocalCharacterPosition(ModelMazeData.Info.Path[0]);
            SetPosition(pos);
        }

        public void OnStartChangePosition(CharacterMovingEventArgs _Args)
        {
            // m_PrevPos = CoordinateConverter.ToLocalCharacterPosition(_Args.From);
            // m_NextPos = CoordinateConverter.ToLocalCharacterPosition(_Args.To);
        }

        public void OnMoving(CharacterMovingEventArgs _Args)
        {
            m_PrevPos = CoordinateConverter.ToLocalCharacterPosition(_Args.From);
            m_NextPos = CoordinateConverter.ToLocalCharacterPosition(_Args.To);
            var pos = Vector2.Lerp(m_PrevPos, m_NextPos, _Args.Progress);
            SetPosition(pos);
            var item = ViewMazeCommon.MazeItems.FirstOrDefault(_Item => _Item.Props.Position == _Args.Current);
            if (item == null)
                return;
            if (item.Props.IsNode)
                (item as ViewMazeItemProt).shape.Color = new Color(1f, 0.93f, 0.51f);
        }

        public void OnDeath()
        {
            
        }

        public void OnHealthChanged(HealthPointsEventArgs _Args)
        {
            
        }

        #endregion
        
        #region nonpublic methods
        
        private void InitShape(float _Radius, Color _Color)
        {
            var go = ContainersGetter.CharacterContainer.gameObject;
            m_Shape = go.GetComponent<Disc>() ?? go.AddComponent<Disc>();
            m_Shape.Radius = _Radius;
            m_Shape.Color = _Color;
            m_Shape.SortingOrder = 100;
        }

        private void SetPosition(Vector2 _Position)
        {
            ContainersGetter.CharacterContainer.localPosition = _Position;
        }
        
        #endregion
    }
}