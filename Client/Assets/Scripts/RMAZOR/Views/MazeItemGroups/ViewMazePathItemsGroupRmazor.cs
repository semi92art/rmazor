using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Extensions;
using Common.Helpers;
using RMAZOR.Helpers;
using RMAZOR.Models;
using RMAZOR.Views.Helpers.MazeItemsCreators;
using RMAZOR.Views.MazeItems.ViewMazeItemPath;
using UnityEngine;

namespace RMAZOR.Views.MazeItemGroups
{
    public class ViewMazePathItemsGroupRmazor : ViewMazePathItemsGroup
    {
        private Dictionary<V2Int, bool> m_PathItemFilledDict;
        private V2Int[]                 m_LastPath;
        
        protected ViewMazePathItemsGroupRmazor(
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
                _MoneyCounter) { }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            switch (_Args.LevelStage)
            {
                case ELevelStage.Loaded:
                    UpdatePathItemFilledDictionary();
                    break;
                case ELevelStage.CharacterKilled:
                    foreach (var item in GetItemsToStopHighlightOnMoveBreak())
                        item.HighlightEnabled = false;
                    break;
            }
            if (_Args.LevelStage != ELevelStage.CharacterKilled)
                return;
            foreach (var item in GetItemsToStopHighlightOnMoveBreak())
                item.HighlightEnabled = false;
        }

        public override void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            m_LastPath = RmazorUtils.GetFullPath(_Args.From, _Args.To);
            var pathItems = RmazorUtils.GetFullPath(_Args.From, _Args.To)
                .Select(_Pos => PathItems.First(
                    _Item => _Item.Props.Position == _Pos && _Item.ActivatedInSpawnPool))
                .Cast<IViewMazeItemPathRmazor>();
            int k = 0;
            foreach (var item in pathItems)
            {
                if (ViewSettings.highlightPathItem)
                    item.HighlightPathItem(++k / ModelSettings.characterSpeed); 
                m_PathItemFilledDict[item.Props.Position] = true;
                item.OnCharacterMoveStarted(_Args);
            }
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

        private IEnumerable<IViewMazeItemPathRmazor> GetItemsToStopHighlightOnMoveBreak()
        {
            var clothestPathItemToCharacter = GetPathItemClosestToCharacterPrecisedPosition();
            var pathItemsToShopHighlight = new List<IViewMazeItemPathRmazor>();
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
                pathItemsToShopHighlight.Add(item as IViewMazeItemPathRmazor);
            }
            return pathItemsToShopHighlight;
        }
        
        private void UpdatePathItemFilledDictionary()
        {
            m_PathItemFilledDict = Model.PathItemsProceeder.PathProceeds.CloneAlt();
        }
    }
}