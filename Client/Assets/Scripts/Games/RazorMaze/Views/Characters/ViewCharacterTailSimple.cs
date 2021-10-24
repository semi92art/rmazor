using System.Collections;
using DI.Extensions;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.Characters
{
    public class ViewCharacterTailSimple : IViewCharacterTail
    {
        #region nonpublic members

        private Triangle m_Tail;
        private bool m_Hiding;
        private bool m_Activated;
        
        #endregion
        
        #region inject

        private ICoordinateConverter CoordinateConverter { get; }
        private IContainersGetter ContainersGetter { get; }
        private ModelSettings ModelSettings { get; }
        private IGameTicker GameTicker { get; }

        public ViewCharacterTailSimple(
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            ModelSettings _ModelSettings,
            IGameTicker _GameTicker)
        {
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter = _ContainersGetter;
            ModelSettings = _ModelSettings;
            GameTicker = _GameTicker;
        }
        
        #endregion
        
        #region api
        
        public event NoArgsHandler Initialized;

        public bool Activated
        {
            get => m_Activated;
            set
            {
                m_Activated = value;
                if (!m_Tail.IsNull())
                    m_Tail.enabled = value;
            }
        }

        public void Init()
        {
            var go = new GameObject("Character Tail");
            go.SetParent(ContainersGetter.CharacterContainer);
            m_Tail = go.AddComponent<Triangle>();
            m_Tail.Color = DrawingUtils.ColorCharacterTail;
            m_Tail.enabled = false;
            Initialized?.Invoke();
        }

        public void ShowTail(CharacterMovingEventArgs _Args)
        {
            m_Hiding = false;
            m_Tail.enabled = true;
            var dir = (_Args.To - _Args.From).Normalized;
            var a = _Args.From.ToVector2() - dir * 0.4f;
            var orth = new Vector2(dir.y, dir.x);
            var currPos = Vector2.Lerp(_Args.From.ToVector2(), _Args.To.ToVector2(), _Args.Progress);
            var b = currPos - dir * 0.4f + orth * 0.3f;
            var c = currPos - dir * 0.4f - orth * 0.3f;
            m_Tail.A = CoordinateConverter.ToLocalCharacterPosition(a);
            m_Tail.B = CoordinateConverter.ToLocalCharacterPosition(b);
            m_Tail.C = CoordinateConverter.ToLocalCharacterPosition(c);
            m_Tail.Roundness = 0.4f;
            m_Tail.gameObject.transform.SetPosXY(CoordinateConverter.GetCenter());
        }

        public void HideTail(CharacterMovingEventArgs _Args = null)
        {
            m_Tail.enabled = _Args != null;
            if (_Args != null)                
                Coroutines.Run(HideTailCoroutine(_Args));
        }

        #endregion

        #region nonpublic methods

        private IEnumerator HideTailCoroutine(CharacterMovingEventArgs _Args)
        {
            m_Hiding = true;
            var startA = (Vector2)m_Tail.A;
            var dir = (_Args.To - _Args.From).Normalized;
            var finishA = CoordinateConverter.ToLocalCharacterPosition(_Args.To.ToVector2() - dir * 0.4f);
            var distance = V2Int.Distance(_Args.From, _Args.To);
            yield return Coroutines.Lerp(
                0f,
                1f,
                distance / ModelSettings.CharacterSpeed,
                _Progress =>
                {
                    m_Tail.A = Vector2.Lerp(startA, finishA, _Progress);
                },
                GameTicker,
                (_Breaked, _Progress) => m_Tail.enabled = false,
                () => !m_Hiding);
        }

        #endregion
    }
}