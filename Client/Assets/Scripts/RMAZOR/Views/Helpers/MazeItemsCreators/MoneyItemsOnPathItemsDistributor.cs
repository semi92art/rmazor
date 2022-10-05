using System.Collections.Generic;
using Common.Helpers;
using RMAZOR.Models.MazeInfos;
using UnityEngine;

namespace RMAZOR.Views.Helpers.MazeItemsCreators
{
    public interface IMoneyItemsOnPathItemsDistributor
    {
        IList<int> GetMoneyItemIndices(MazeInfo _Info);
    }
    
    public class MoneyItemsOnPathItemsDistributor : IMoneyItemsOnPathItemsDistributor
    {
        #region inject
        
        private GlobalGameSettings GlobalGameSettings { get; }

        public MoneyItemsOnPathItemsDistributor(GlobalGameSettings _GlobalGameSettings)
        {
            GlobalGameSettings = _GlobalGameSettings;
        }

        #endregion

        #region api
        
        public IList<int> GetMoneyItemIndices(MazeInfo _Info)
        {
            int pathItemsCount = _Info.PathItems.Count;
            if (pathItemsCount < 10)
                return new List<int>();
            float rate = GlobalGameSettings.moneyItemsRate;
            int moneyItemsCount = Mathf.FloorToInt(_Info.PathItems.Count * rate);
            var indices = new List<int>(moneyItemsCount);
            for (int i = 0; i < moneyItemsCount; i++)
            {
                int index = -1;
                while (index == -1 || indices.Contains(index))
                    index = Mathf.FloorToInt(Random.value * pathItemsCount);
                indices.Add(index);
                
            }
            return indices;
        }
        
        #endregion
    }
}