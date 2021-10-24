using System.Collections;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Ticker;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Games.RazorMaze.Views.Common
{
    public interface IMazeShaker : IInit
    {
        bool ShakeMaze { get; set; }
        IEnumerator HitMazeCoroutine(CharacterMovingEventArgs _Args);
        IEnumerator ShakeMazeCoroutine();
    }
    
    public class MazeShaker : IMazeShaker, IUpdateTick
    {
        #region nonpublic members

        private Transform m_MazeContainer;
        private bool m_Initialized;
        private Vector3 m_StartPosition;
        private bool m_ShakeMaze;

        #endregion
        
        #region inject

        private IContainersGetter ContainersGetter { get; }
        private IGameTicker GameTicker { get; }

        public MazeShaker(IContainersGetter _ContainersGetter, IGameTicker _GameTicker)
        {
            ContainersGetter = _ContainersGetter;
            GameTicker = _GameTicker;
            _GameTicker.Register(this);
        }

        #endregion

        #region api

        public bool ShakeMaze
        {
            get => m_ShakeMaze;
            set
            {
                m_ShakeMaze = value;
                if (value)
                    m_StartPosition = m_MazeContainer.position;
                else
                    m_MazeContainer.position = m_StartPosition;
            }
        }
        public event UnityAction Initialized;
        
        public void Init()
        {
            m_MazeContainer = ContainersGetter.GetContainer(ContainerNames.Maze);
            Initialized?.Invoke();
            m_Initialized = true;
        }

        public IEnumerator HitMazeCoroutine(CharacterMovingEventArgs _Args)
        {
            const float amplitude = 0.5f;
            var dir = RazorMazeUtils.GetDirectionVector(_Args.Direction, MazeOrientation.North);
            Vector2 startPos = m_MazeContainer.position;
            const float duration = 0.1f;
            yield return Coroutines.Lerp(
                0f,
                1f,
                duration,
                _Progress =>
                {
                    float distance = _Progress < 0.5f ? _Progress * amplitude : (1f - _Progress) * amplitude;
                    var res = startPos + distance * dir.ToVector2();
                    m_MazeContainer.position = res;
                },
                GameTicker,
                (_Finished, _Progress) =>
                {
                    m_MazeContainer.position = startPos;
                });
        }

        public IEnumerator ShakeMazeCoroutine()
        {
            var startPos = m_MazeContainer.position;
            const float duration = 0.5f;
            yield return Coroutines.Lerp(
                1f,
                0f,
                duration,
                _Progress =>
                {
                    float amplitude = 0.25f * _Progress;
                    Vector2 res;
                    res.x = startPos.x + amplitude * Mathf.Sin(GameTicker.Time * 200f);
                    res.y = startPos.y + amplitude * Mathf.Cos(GameTicker.Time * 100f);
                    m_MazeContainer.position = res;
                },
                GameTicker,
                (_Finished, _Progress) =>
                {
                    m_MazeContainer.position = startPos;
                });
        }
        
        public void UpdateTick()
        {
            if (!m_Initialized)
                return;
            if (!m_ShakeMaze)
                return;
            float amplitude = 0.25f;
            Vector2 res;
            res.x = m_StartPosition.x + amplitude * Mathf.Sin(GameTicker.Time * 200f);
            res.y = m_StartPosition.y + amplitude * Mathf.Cos(GameTicker.Time * 100f);
            m_MazeContainer.position = res;
        }

        #endregion
    }
}