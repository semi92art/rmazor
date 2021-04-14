using System.Collections;
using Entities;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Utils;
using Shapes;
using TimeProviders;
using UnityEngine;
using Utils;
using Zenject;

namespace Games.RazorMaze.Views.Characters
{
    public class ViewCharacterTailSimple : IViewCharacterTail
    {

        #region nonpublic members

        private Triangle m_Tail;
        
        #endregion
        
        #region inject

        private IModelMazeData Data { get; }
        private ICoordinateConverter CoordinateConverter { get; }
        private IContainersGetter ContainersGetter { get; }
        private ModelSettings ModelSettings { get; }
        private ITimeProvider GameTimeProvider { get; }
        
        public ViewCharacterTailSimple(
            IModelMazeData _Data,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            ModelSettings _ModelSettings,
            [Inject(Id = "Game")] ITimeProvider _GameTimeProvider)
        {
            Data = _Data;
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter = _ContainersGetter;
            ModelSettings = _ModelSettings;
            GameTimeProvider = _GameTimeProvider;
        }
        
        #endregion

        public void Init()
        {
            var go = new GameObject("Character Tail");
            go.SetParent(ContainersGetter.CharacterContainer);
            m_Tail = go.AddComponent<Triangle>();
            m_Tail.Color = ViewUtils.ColorCharacterTail;
            m_Tail.enabled = false;
        }

        public void ShowTail(CharacterMovingEventArgs _Args)
        {
            m_Tail.enabled = true;
            var dir = (_Args.To - _Args.From).Normalized;
            var a = _Args.From.ToVector2() - dir * 0.4f;
            var orth = new Vector2(dir.y, dir.x);
            var currPos = Vector2.Lerp(_Args.From.ToVector2(), _Args.To.ToVector2(), _Args.Progress);
            var b = currPos + dir * 0.4f + orth * 0.3f;
            var c = currPos + dir * 0.4f - orth * 0.3f;
            m_Tail.A = CoordinateConverter.ToLocalCharacterPosition(a);
            m_Tail.B = CoordinateConverter.ToLocalCharacterPosition(b);
            m_Tail.C = CoordinateConverter.ToLocalCharacterPosition(c);
            m_Tail.gameObject.transform.SetPosXY(CoordinateConverter.GetCenter());
        }

        public void HideTail(CharacterMovingEventArgs _Args)
        {
            Coroutines.Run(HideTailCoroutine(_Args));
        }

        private IEnumerator HideTailCoroutine(CharacterMovingEventArgs _Args)
        {
            var startA = (Vector2)m_Tail.A;
            var dir = (_Args.To - _Args.From).Normalized;
            var finishA = CoordinateConverter.ToLocalCharacterPosition(_Args.To.ToVector2() - dir * 0.4f);
            var distance = V2Int.Distance(_Args.From, _Args.To);
            yield return Coroutines.Lerp(
                0f,
                1f,
                distance / ModelSettings.characterSpeed,
                _Progress =>
                {
                    m_Tail.A = Vector2.Lerp(startA, finishA, _Progress);
                },
                GameTimeProvider,
                (_Breaked, _Progress) => m_Tail.enabled = false);
        }
    }
}