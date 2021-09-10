using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeCommon;
using Shapes;
using UnityEngine;

namespace Games.RazorMaze.Views.Characters
{
    public class ViewCharacterProt : IViewCharacter
    {
        #region nonpublic members
        
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

        public event NoArgsHandler Initialized;

        public void Init()
        {
            CoordinateConverter.Init(ModelMazeData.Info.Size);
            InitShape(0.4f * CoordinateConverter.GetScale(), new Color(1f, 0.38f, 0f));
            var pos = CoordinateConverter.ToLocalCharacterPosition(ModelMazeData.Info.Path[0]);
            SetPosition(pos);
            Initialized?.Invoke();
        }

        public void OnMovingStarted(CharacterMovingEventArgs _Args) { }

        public void OnMoving(CharacterMovingEventArgs _Args)
        {
            var prevPos = CoordinateConverter.ToLocalCharacterPosition(_Args.From);
            var nextPos = CoordinateConverter.ToLocalCharacterPosition(_Args.To);
            var pos = Vector2.Lerp(prevPos, nextPos, _Args.Progress);
            SetPosition(pos);
        }

        public void OnMovingFinished(CharacterMovingEventArgs _Args) { }

        public void OnDeath() { }

        public void OnHealthChanged(HealthPointsEventArgs _Args) { }

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

        private void SetPosition(Vector2 _Position)
        {
            ContainersGetter.CharacterContainer.localPosition = _Position;
        }
        
        #endregion
    }
}