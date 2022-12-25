using Common.Helpers;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
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
            if (_Args.LevelStage != ELevelStage.Loaded)
                return;
            SetColorsOnNewLevel(_Args);
        }

        protected virtual void SetColorsOnNewLevel(LevelStageArgs _Args) { }
    }
}