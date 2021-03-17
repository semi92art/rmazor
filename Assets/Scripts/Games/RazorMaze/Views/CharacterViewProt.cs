using Entities;
using Games.RazorMaze.Models;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views
{
    public class CharacterViewProt : ICharacterView
    {
        private Vector2 m_PrevPos;
        private Vector2 m_NextPos;
        private CharacterViewItemProt m_Item;
        private MazeInfo m_Info;
        
        public void Init(MazeInfo _Info)
        {
            m_Info = _Info;
            var parent = CommonUtils.FindOrCreateGameObject("Maze", out _);
            m_Item = CharacterViewItemProt.Create(parent.transform, _Info);
        }

        public void OnStartChangePosition(V2Int _PrevPos, V2Int _NextPos)
        {
            var scaler = new MazeScreenScaler();
            m_PrevPos = scaler.GetCharacterPosition(_PrevPos, m_Info.Width, out _);
            m_NextPos = scaler.GetCharacterPosition(_NextPos, m_Info.Width, out _);
        }

        public void OnMoving(float _Progress)
        {
            float coeff = MoveCoefficient(_Progress);
            var pos = Vector2.Lerp(m_PrevPos, m_NextPos, coeff);
            m_Item.SetPosition(pos);
        }

        public void OnDeath()
        {
            //throw new System.NotImplementedException();
        }

        public void OnHealthChanged(HealthPointsEventArgs _Args)
        {
            //throw new System.NotImplementedException();
        }
        
        private static float MoveCoefficient(float _Progress) => Mathf.Pow(_Progress, 2);
    }
}