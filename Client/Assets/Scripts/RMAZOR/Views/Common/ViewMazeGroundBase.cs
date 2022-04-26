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
            if (_Args.Stage != ELevelStage.Loaded)
                return;
            SetColorsOnNewLevel(_Args.LevelIndex);
        }

        protected virtual void SetColorsOnNewLevel(long _LevelIndex) { }
    }
}