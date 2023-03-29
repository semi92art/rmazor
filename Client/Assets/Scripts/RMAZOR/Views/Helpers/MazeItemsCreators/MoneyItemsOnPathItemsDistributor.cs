using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using mazing.common.Runtime.Entities;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.Utils;
using UnityEngine;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.Helpers.MazeItemsCreators
{
    public interface IMoneyItemsOnPathItemsDistributor
    {
        IList<V2Int> GetMoneyItemPoints(MazeInfo _Info);
    }
    
    public class MoneyItemsOnPathItemsDistributor : IMoneyItemsOnPathItemsDistributor
    {
        #region inject
        
        private GlobalGameSettings GlobalGameSettings { get; }
        private IModelGame         Model              { get; }

        public MoneyItemsOnPathItemsDistributor(
            GlobalGameSettings _GlobalGameSettings,
            IModelGame         _Model)
        {
            GlobalGameSettings = _GlobalGameSettings;
            Model              = _Model;
        }

        #endregion

        #region api
        
        public IList<V2Int> GetMoneyItemPoints(MazeInfo _Info)
        {
            float rateCoefficient = 1;
            string gameMode = ViewLevelStageSwitcherUtils.GetGameMode(Model.LevelStaging.Arguments);
            string levelType = ViewLevelStageSwitcherUtils.GetNextLevelType(Model.LevelStaging.Arguments);
            if (gameMode == ParameterGameModeMain && levelType == ParameterLevelTypeBonus)
            {
                rateCoefficient = 1;
            }
            int pathItemsCount = _Info.PathItems.Count;
            if (pathItemsCount < 10)
                return new List<V2Int>();
            float rate = GlobalGameSettings.moneyItemsRate * rateCoefficient;
            int moneyItemsCount = Mathf.FloorToInt(_Info.PathItems.Count * rate);
            var indices = new List<int>(moneyItemsCount);
            for (int i = 0; i < moneyItemsCount; i++)
            {
                int index = -1;
                while (index == -1 || indices.Contains(index))
                    index = Mathf.FloorToInt(Random.value * pathItemsCount);
                indices.Add(index);
            }
            var points = new List<V2Int>();
            var pathItems = _Info.PathItems;
            foreach (int index in indices)
            {
                var pathItem = pathItems[index];
                if (IsPointOnMovingMazeItemPathLine(_Info, pathItem.Position))
                    continue;
                if (IsPointOnKeyLock(_Info, pathItem.Position))
                    continue;
                points.Add(pathItem.Position);
            }
            return points;
        }
        
        #endregion

        #region nonpublic methods

        private static bool IsPointOnMovingMazeItemPathLine(MazeInfo _Info, V2Int _Point)
        {
            var movingItemTypes = new[] {EMazeItemType.TrapMoving, EMazeItemType.GravityBlock};
            foreach (var mazeItem in _Info.MazeItems.Where(_Item => movingItemTypes.Contains(_Item.Type)))
            {
                var path = mazeItem.Path;
                for (int i = 0; i < path.Count - 1; i++)
                {
                    var pt1 = path[i];
                    var pt2 = path[i + 1];
                    var pathLinePoints = RmazorUtils.GetFullPath(pt1, pt2);
                    if (pathLinePoints.Contains(_Point))
                        return true;
                }
            }

            return false;
        }

        private static bool IsPointOnKeyLock(MazeInfo _Info, V2Int _Point)
        {
            return _Info.MazeItems.Any(
                _Item => _Item.Type == EMazeItemType.KeyLock 
                         && _Item.Position == _Point);
        }

        #endregion
    }
}