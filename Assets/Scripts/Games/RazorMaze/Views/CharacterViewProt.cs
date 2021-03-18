using Entities;
using Games.RazorMaze.Models;
using Shapes;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views
{
    public class CharacterViewProt : ICharacterView
    {
        private ICoordinateConverter CoordinateConverter { get; }
        private IMazeModel MazeModel { get; }
        
        private Vector2 m_PrevPos;
        private Vector2 m_NextPos;

        private Disc m_Shape;
        private Transform m_ItemTransform;


        public CharacterViewProt(ICoordinateConverter _CoordinateConverter, IMazeModel _MazeModel)
        {
            CoordinateConverter = _CoordinateConverter;
            MazeModel = _MazeModel;
        }
        
        public void Init()
        {
            CoordinateConverter.Init(MazeModel.Info.Width);
            var parent = CommonUtils.FindOrCreateGameObject("Maze", out _);
            m_ItemTransform = new GameObject("Character Item").transform;
            m_ItemTransform.SetParent(parent.transform);
            InitShape(0.4f * CoordinateConverter.GetScale(), Color.blue);
            var pos = CoordinateConverter.ToLocalPosition(MazeModel.Info.Nodes[0].Position);
            SetPosition(pos);
        }

        public void OnStartChangePosition(V2Int _PrevPos, V2Int _NextPos)
        {
            m_PrevPos = CoordinateConverter.ToLocalPosition(_PrevPos);
            m_NextPos = CoordinateConverter.ToLocalPosition(_NextPos);
        }

        public void OnMoving(float _Progress)
        {
            float coeff = MoveCoefficient(_Progress);
            var pos = Vector2.Lerp(m_PrevPos, m_NextPos, coeff);
            SetPosition(pos);
        }

        public void OnDeath()
        {
            throw new System.NotImplementedException();
        }

        public void OnHealthChanged(HealthPointsEventArgs _Args)
        {
            throw new System.NotImplementedException();
        }
        
        private static float MoveCoefficient(float _Progress) => Mathf.Pow(_Progress, 2);
        
        private void InitShape(float _Radius, Color _Color)
        {
            var go = m_ItemTransform.gameObject;
            m_Shape = go.GetComponent<Disc>() ?? go.AddComponent<Disc>();
            m_Shape.Radius = _Radius;
            m_Shape.Color = _Color;
            m_Shape.SortingOrder = 1;
        }

        private void SetPosition(Vector2 _Position)
        {
            m_ItemTransform.localPosition = _Position;
        }
    }
}