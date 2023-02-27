using mazing.common.Runtime;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;

namespace RMAZOR.Helpers
{
    public interface ILevelsLoader : IInit
    {
        string           GetLevelInfoRaw(LevelInfoArgs _Args);
        Entity<MazeInfo> GetLevelInfo(LevelInfoArgs _Args);
        int              GetLevelsCount(LevelInfoArgs _Args);
    }
    
    public abstract class LevelsLoader : InitBase, ILevelsLoader
    {
        #region nonpublic members

        protected static string PrefabSetName => $"game_{CommonData.GameId}_levels";

        #endregion
        
        #region inject

        protected IPrefabSetManager  PrefabSetManager  { get; }
        protected IMazeInfoValidator MazeInfoValidator { get; }

        protected LevelsLoader(
            IPrefabSetManager  _PrefabSetManager,
            IMazeInfoValidator _MazeInfoValidator)
        {
            PrefabSetManager  = _PrefabSetManager;
            MazeInfoValidator = _MazeInfoValidator;
        }

        #endregion

        #region api
        public abstract Entity<MazeInfo> GetLevelInfo(LevelInfoArgs    _Args);
        public abstract string           GetLevelInfoRaw(LevelInfoArgs _Args);
        public abstract int              GetLevelsCount(LevelInfoArgs  _Args);

        #endregion

        #region nonpublic methods
        


        protected static string LevelsAssetName(int _HeapIndex)
        {
            string heapName = _HeapIndex <= 0 ? null : $"levels_{_HeapIndex}";
            return heapName ?? $"levels_{_HeapIndex}";
        }

        #endregion
    }
}