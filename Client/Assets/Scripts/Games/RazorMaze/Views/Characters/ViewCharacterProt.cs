using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Shapes;
using UnityEngine;

namespace Games.RazorMaze.Views.Characters
{
    public class ViewCharacterProt : ViewCharacterBase
    {
        #region nonpublic members
        
        private Disc m_Shape;
        
        #endregion
     
        #region inject
        
        public ViewCharacterProt(
            ICoordinateConverter _CoordinateConverter, 
            IModelMazeData _ModelMazeData, 
            IContainersGetter _ContainersGetter,
            IViewMazeCommon _ViewMazeCommon)
            : base(_CoordinateConverter, _ModelMazeData, _ContainersGetter, _ViewMazeCommon)
        { }
        
        #endregion
        
        #region api
        
        public override void Init()
        {
            InitShape(0.4f * CoordinateConverter.GetScale(), new Color(1f, 0.38f, 0f));
            base.Init();
        }
        
        public override void OnMoving(CharacterMovingEventArgs _Args)
        {
            var prevPos = CoordinateConverter.ToLocalCharacterPosition(_Args.From);
            var nextPos = CoordinateConverter.ToLocalCharacterPosition(_Args.To);
            var pos = Vector2.Lerp(prevPos, nextPos, _Args.Progress);
            SetPosition(pos);
        }

        #endregion
        
        #region nonpublic methods
        
        private void InitShape(float _Radius, Color _Color)
        {
            var go = ContainersGetter.CharacterContainer.gameObject;
            m_Shape = go.GetOrAddComponent<Disc>();
            m_Shape.Radius = _Radius;
            m_Shape.Color = _Color;
            m_Shape.SortingOrder = 100;
        }

        #endregion

        
    }
}