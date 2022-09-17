using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Extensions;
using Common.Helpers;
using Common.Ticker;
using Common.Utils;
using Shapes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR
{
    public interface IRendererAppearTransitioner : IInit
    {
        void DoAppearTransition(
            bool                                            _Appear,
            Dictionary<IEnumerable<Component>, Func<Color>> _Sets,
            float                                           _Delay,
            UnityAction                                     _OnFinish = null);
    }

    public class RendererAppearTransitioner : InitBase, IRendererAppearTransitioner
    {
        #region inject

        private IViewGameTicker GameTicker   { get; }

        private RendererAppearTransitioner(IViewGameTicker _GameTicker)
        {
            GameTicker = _GameTicker;
        }

        #endregion

        #region api

        public void DoAppearTransition(
            bool                                            _Appear,
            Dictionary<IEnumerable<Component>, Func<Color>> _Sets,
            float                                           _Delay,
            UnityAction                                     _OnFinish = null)
        {
            Cor.Run(WaitWhileAndNextFrame(_Appear ? 0f : _Delay + 0.01f, _OnFinish));
            if (_Sets == null)
                return;
            foreach (var set in _Sets)
            {
                var endCol = !_Appear ? () => set.Value().SetA(0f) : set.Value;
                var shapes = set.Key.Where(_Shape => _Shape.IsNotNull()).ToList();
                void FastTransit()
                {
                    foreach (var shape in shapes)
                    {
                        switch (shape)
                        {
                            case Behaviour beh: beh.enabled = true; break;
                            case Renderer rend: rend.enabled = true; break;
                        }
                        switch (shape)
                        {
                            case ShapeRenderer shapeRenderer:   shapeRenderer.Color  = endCol(); break;
                            case SpriteRenderer spriteRenderer: spriteRenderer.color = endCol(); break;
                            case TextMeshPro textMeshPro:       textMeshPro.color    = endCol(); break;
                        }
                    }
                }
                Cor.Run(Cor.Delay(
                    _Appear ? 0f : _Delay,
                    GameTicker,
                    FastTransit));
            }
        }

        private IEnumerator WaitWhileAndNextFrame(float _Delay, UnityAction _Action)
        {
            yield return null;
            yield return Cor.Delay(
                _Delay,
                GameTicker,
                _Action);
        }

        #endregion
    }
}