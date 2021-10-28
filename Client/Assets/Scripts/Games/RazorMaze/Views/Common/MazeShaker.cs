using System.Collections;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeItems;
using Shapes;
using Ticker;
using TMPro;
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
        void OnCharacterDeathAnimation(
            Vector2 _DeathPosition,
            IViewMazeItem _MazeItem);
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
        private IMazeCoordinateConverter CoordinateConverter { get; }
        private IGameTicker GameTicker { get; }

        public MazeShaker(
            IContainersGetter _ContainersGetter, 
            IMazeCoordinateConverter _CoordinateConverter,
            IGameTicker _GameTicker)
        {
            ContainersGetter = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
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
            const float duration = 1f;
            yield return Coroutines.Lerp(
                1f,
                0f,
                duration,
                _Progress =>
                {
                    float amplitude = 0.5f * _Progress;
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
        
         public void OnCharacterDeathAnimation(
            Vector2 _DeathPosition,
            IViewMazeItem _MazeItem)
        {
            const float scaleCoeff = 0.2f;
            const float maxDistance = 10f;
            const float transitionTime = 0.5f;

            float dist = Vector2.Distance(_DeathPosition, _MazeItem.Object.transform.position);
            if (dist > maxDistance * CoordinateConverter.Scale)
                return;
            
            foreach (var shape in _MazeItem.Shapes)
            {
                Coroutines.Run(Coroutines.Delay(
                    () =>
                    {
                        Coroutines.Run(Coroutines.Lerp(
                            0f,
                            1f,
                            transitionTime,
                            _Progress =>
                            {
                                var scale = Vector3.one * (1f + _Progress * scaleCoeff);
                                if (shape is ShapeRenderer shapeRenderer)
                                    shapeRenderer.transform.localScale = scale;
                                else if (shape is SpriteRenderer spriteRenderer)
                                    spriteRenderer.transform.localScale = scale;
                                else if (shape is TextMeshPro textMeshPro)
                                    textMeshPro.transform.localScale = scale;
                            },
                            GameTicker,
                            (_, __) =>
                            {
                                if (shape is ShapeRenderer shapeRenderer)
                                    shapeRenderer.transform.localScale = Vector3.one;
                                else if (shape is SpriteRenderer spriteRenderer)
                                    spriteRenderer.transform.localScale = Vector3.one;
                                else if (shape is TextMeshPro textMeshPro)
                                    textMeshPro.transform.localScale = Vector3.one;
                            },
                            _ProgressFormula: _P => _P < 0.5f ? 2f * _P : 2f * (1f - _P)));
                    }, Mathf.Max(0f, (dist - 1) * 0.005f)));
            }
        }

        #endregion
    }
}