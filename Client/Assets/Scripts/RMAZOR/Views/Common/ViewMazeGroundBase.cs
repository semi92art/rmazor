using Common.Helpers;
using Common.Providers;
using RMAZOR.Models;

namespace RMAZOR.Views.Common
{
    public abstract class ViewMazeGroundBase : InitBase, IOnLevelStageChanged
    {
        protected readonly IColorProvider ColorProvider;

        protected ViewMazeGroundBase(IColorProvider _ColorProvider)
        {
            ColorProvider = _ColorProvider;
        }

        public virtual void OnLevelStageChanged(LevelStageArgs _Args)
        {
            SetColorsOnNewLevel(_Args.LevelIndex);
        }

        protected virtual void SetColorsOnNewLevel(long _LevelIndex) { }
    }
}