using Entities;
using Games.RazorMaze.Models;
using Shapes;
using UnityEngine;

namespace Games.RazorMaze.Views
{
    public class CharacterViewProt : ICharacterView
    {
        private ICoordinateConverter CoordinateConverter { get; }
        private IMazeModel MazeModel { get; }
        public IContainersGetter ContainersGetter { get; }

        private Vector2 m_PrevPos;
        private Vector2 m_NextPos;
        private Disc m_Shape;
        
        public CharacterViewProt(
            ICoordinateConverter _CoordinateConverter, 
            IMazeModel _MazeModel, 
            IContainersGetter _ContainersGetter)
        {
            CoordinateConverter = _CoordinateConverter;
            MazeModel = _MazeModel;
            ContainersGetter = _ContainersGetter;
        }
        
        public void Init()
        {
            CoordinateConverter.Init(MazeModel.Info.Width);
            InitShape(0.4f * CoordinateConverter.GetScale(), new Color(1f, 0.38f, 0f));
            var pos = CoordinateConverter.ToLocalCharacterPosition(MazeModel.Info.Nodes[0].Position);
            SetPosition(pos);
        }

        public void OnStartChangePosition(V2Int _PrevPos, V2Int _NextPos)
        {
            m_PrevPos = CoordinateConverter.ToLocalCharacterPosition(_PrevPos);
            m_NextPos = CoordinateConverter.ToLocalCharacterPosition(_NextPos);
        }

        public void OnMoving(float _Progress)
        {
            float coeff = MoveCoefficient(_Progress);
            var pos = Vector2.Lerp(m_PrevPos, m_NextPos, coeff);
            SetPosition(pos);
        }

        public void OnDeath()
        {
            
        }

        public void OnHealthChanged(HealthPointsEventArgs _Args)
        {
            
        }
        
        private static float MoveCoefficient(float _Progress) => Mathf.Pow(_Progress, 2);
        
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
    }
}