using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Helpers;
using RMAZOR.Helpers;
using RMAZOR.Models;
using RMAZOR.Views.Helpers.MazeItemsCreators;
using RMAZOR.Views.MazeItems;
using UnityEngine;

namespace RMAZOR.Views.MazeItemGroups
{
    public class ViewMazePathItemsFilledWithAdditionalBordersGroup : ViewMazePathItemsGroup
    {
        private V2Int[] m_LastPath;
        
        protected ViewMazePathItemsFilledWithAdditionalBordersGroup(
            GlobalGameSettings _GlobalGameSettings,
            ViewSettings       _ViewSettings,
            ModelSettings      _ModelSettings,
            IModelGame         _Model,
            IMazeItemsCreator  _MazeItemsCreator,
            IMoneyCounter      _MoneyCounter)
            : base(
                _GlobalGameSettings, 
                _ViewSettings, 
                _ModelSettings, 
                _Model,
                _MazeItemsCreator,
                _MoneyCounter)
        {
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            if (_Args.LevelStage != ELevelStage.CharacterKilled)
                return;
            foreach (var item in GetItemsToStopHighlightOnMoveBreak())
                item.HighlightEnabled = false;
        }

        public override void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            m_LastPath = RmazorUtils.GetFullPath(_Args.From, _Args.To);
            base.OnCharacterMoveStarted(_Args);
        }

        public override void OnPathCompleted(V2Int _LastPath)
        {
            foreach (var item in GetItemsToStopHighlightOnMoveBreak())
                item.HighlightEnabled = false;
            base.OnPathCompleted(_LastPath);
        }

        private V2Int GetPathItemClosestToCharacterPrecisedPosition()
        {
            float minDistance = float.PositiveInfinity;
            V2Int clothestPathItem = default;
            var charPos = Model.Character.IsMoving
                ? Model.Character.MovingInfo.PrecisePosition
                : Model.Character.Position;
            foreach (var pathItem in m_LastPath)
            {
                float distance = Vector2.Distance(pathItem, charPos);
                if (distance > minDistance)
                    continue;
                minDistance = distance;
                clothestPathItem = pathItem;
            }
            return clothestPathItem;
        }

        private IEnumerable<IViewMazeItemPathFilled> GetItemsToStopHighlightOnMoveBreak()
        {
            var clothestPathItemToCharacter = GetPathItemClosestToCharacterPrecisedPosition();
            var pathItemsToShopHighlight = new List<IViewMazeItemPathFilled>();
            foreach (var pathItemPos in m_LastPath)
            {
                int comp = RmazorUtils.CompareItemsOnPath(
                    m_LastPath, 
                    pathItemPos, 
                    clothestPathItemToCharacter);
                if (comp != 1)
                    continue;
                var item = PathsPool.First(
                    _Item => _Item.ActivatedInSpawnPool && _Item.Props.Position == pathItemPos);
                pathItemsToShopHighlight.Add(item as IViewMazeItemPathFilled);
            }
            return pathItemsToShopHighlight;
        }
    }
}