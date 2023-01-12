using System;
using System.Collections.Generic;
using Common;
using Common.Extensions;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Models.ProceedInfos;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItemKeyLock : IViewMazeItem, ICharacterMoveFinished
    {
        void OnKeyLockStateChanged(IMazeItemProceedInfo _ProceedInfo, bool _ProceedAsLock);
    }
    
    public class ViewMazeItemKeyLock : ViewMazeItemBase, IViewMazeItemKeyLock
    {
        #region nonpublic members
        
        private static AudioClipArgs AudioClipArgsOpenBlock => 
            new AudioClipArgs("shredinger_open", EAudioClipType.GameSound);
        private static AudioClipArgs AudioClipArgsCloseBlock =>
            new AudioClipArgs("shredinger_close", EAudioClipType.GameSound);
        
        protected override string ObjectName => "Key Lock Maze Item";

        private Line      m_KeyLine;
        private Rectangle m_LockShape;

        #endregion

        #region inject
        
        private ViewMazeItemKeyLock(
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            ICoordinateConverter        _CoordinateConverter,
            IContainersGetter           _ContainersGetter,
            IViewGameTicker             _GameTicker,
            IRendererAppearTransitioner _Transitioner,
            IManagersGetter             _Managers,
            IColorProvider              _ColorProvider,
            IViewInputCommandsProceeder _CommandsProceeder)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner, 
                _Managers, 
                _ColorProvider,
                _CommandsProceeder) { }

        #endregion

        #region api

        public override Component[] Renderers => new Component[0];

        public void OnKeyLockStateChanged(IMazeItemProceedInfo _ProceedInfo, bool _ProceedAsLock)
        {
            if (_ProceedAsLock)
                ProceedAsLock(_ProceedInfo.ProceedingStage == ModelCommonData.KeyLockStage2);
            else 
                ProceedAsKey(_ProceedInfo.ProceedingStage == ModelCommonData.KeyLockStage2);
        }

        public override object Clone() =>
            new ViewMazeItemKeyLock(
                ViewSettings, 
                Model, 
                CoordinateConverter,
                ContainersGetter, 
                GameTicker,
                Transitioner,
                Managers,
                ColorProvider,
                CommandsProceeder);
        
        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            if (!IsThisItemLock())
                return;
            const float delta = 0.5f;
            const float duration = 0.1f;
            var startPos = m_LockShape.transform.localPosition;
            Vector2 dir = RmazorUtils.GetDirectionVector(_Args.Direction, Model.MazeRotation.Orientation);
            Cor.Run(Cor.Lerp(
                GameTicker,
                duration * 0.5f,
                0f,
                delta,
                _Progress => m_LockShape.transform.localPosition = startPos + (Vector3) dir * _Progress,
                () =>
                {
                    Cor.Run(Cor.Lerp(
                        GameTicker,
                        duration * 0.5f,
                        delta,
                        0f,
                        _Progress =>
                        {
                            m_LockShape.transform.localPosition = startPos + (Vector3) dir * _Progress;
                        }));
                }));
        }

        #endregion

        #region nonpublic methods
        
        protected override void InitShape()
        {
            m_KeyLine = Object.AddComponentOnNewChild<Line>("Key-Lock Block Key Line", out _)
                .SetSortingOrder(SortingOrders.GetBlockSortingOrder(Props.Type))
                .SetEndCaps(LineEndCap.Round)
                .SetDashed(false);
            m_KeyLine.enabled = false;
            m_LockShape = Object.AddComponentOnNewChild<Rectangle>("Key-Lock Block Lock Shape", out _)
                .SetSortingOrder(SortingOrders.GetBlockSortingOrder(Props.Type))
                .SetType(Rectangle.RectangleType.RoundedSolid)
                .SetDashed(false);
            m_LockShape.enabled = false;
        }

        protected override void UpdateShape()
        {
            if (IsThisItemLock())
                UpdateShapeAsLock();
            else 
                UpdateShapeAsKey();
        }

        private void UpdateShapeAsKey()
        {
            m_KeyLine.enabled = true;
            m_LockShape.enabled = false;
            float scale = CoordinateConverter.Scale;
            (Vector2 start, Vector2 end) = GetKeyLineStartEndPositions(false);
            m_KeyLine
                .SetThickness(scale * ViewSettings.LineThickness)
                .SetStart(start)
                .SetEnd(end);
        }

        private void UpdateShapeAsLock()
        {
            m_KeyLine.enabled = false;
            m_LockShape.enabled = true;
            float scale = CoordinateConverter.Scale;
            m_LockShape
                .SetCornerRadius(scale * ViewSettings.LineThickness)
                .SetWidth(scale * 0.9f)
                .SetHeight(scale * 0.9f);
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId != ColorIds.MazeItem2)
                return;
            m_KeyLine.SetColor(_Color);
            m_LockShape.SetColor(_Color);
        }

        protected override void OnAppearFinish(bool _Appear)
        {
            if (!_Appear)
            {
                m_LockShape.enabled = false;
                m_KeyLine.enabled = false;
            }
            base.OnAppearFinish(_Appear);
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var col = ColorProvider.GetColor(ColorIds.MazeItem2);
            var sets = new Dictionary<IEnumerable<Component>, Func<Color>>();
            if (IsThisItemLock())
                sets.Add(new [] {m_LockShape}, () => col);
            else
                sets.Add(new [] {m_KeyLine}, () => col);
            return sets;
        }

        private void ProceedAsKey(bool _SwitchState2)
        {
            (Vector2 startPrev, Vector2 endPrev) = (m_KeyLine.Start, m_KeyLine.End);
            (Vector2 startNew, Vector2 endNew) = GetKeyLineStartEndPositions(_SwitchState2);
            Cor.Run(Cor.Lerp(GameTicker, 0.1f, _OnProgress: _P =>
            {
                var start = Vector2.Lerp(startPrev, startNew, _P);
                var end = Vector2.Lerp(endPrev, endNew, _P);
                m_KeyLine
                    .SetStart(start)
                    .SetEnd(end);
            }));
        }

        private void ProceedAsLock(bool _Open)
        {
            Managers.AudioManager.PlayClip(_Open ? AudioClipArgsOpenBlock : AudioClipArgsCloseBlock);
            float alphaStart = _Open ? 1f : 0f;
            float alphaEnd = _Open ? 0f : 1f;
            Cor.Run(Cor.Lerp(GameTicker, 0.1f, _OnProgress: _P =>
            {
                float alphaNew = Mathf.Lerp(alphaStart, alphaEnd, _P);
                m_LockShape.Color = m_LockShape.Color.SetA(alphaNew);
            }));
        }

        private Tuple<Vector2, Vector2> GetKeyLineStartEndPositions(bool _Switched)
        {
            Vector2 start, end;
            const float lineLength = 0.8f;
            if (!_Switched)
            {
                start = new Vector2(-1f, -1f);
                end   = new Vector2(1f, 1f);
            }
            else
            {
                start = new Vector2(-1f, 1f);
                end   = new Vector2(1f, -1f);
            }

            float scale = CoordinateConverter.Scale;
            start *= scale * lineLength * .5f;
            end *= scale * lineLength * .5f;
            return new Tuple<Vector2, Vector2>(start, end);
        }

        private bool IsThisItemLock()
        {
            return Props.Directions[0] == V2Int.Right;
        }

        #endregion
    }
}