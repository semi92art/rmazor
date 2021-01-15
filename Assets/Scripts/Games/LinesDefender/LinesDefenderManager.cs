using Managers;

namespace Games.LinesDefender
{
    public class LinesDefenderManager : GameManagerBase
    {
        #region singleton

        public static IGameManager Instance => GetInstance<LinesDefenderManager>();

        #endregion

        protected override float LevelDuration(int _Level)
        {
            return int.MaxValue;
        }

        protected override int NecessaryScore(int _Level)
        {
            return int.MaxValue;
        }
    }
}