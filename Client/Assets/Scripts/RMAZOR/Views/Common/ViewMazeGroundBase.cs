using Common.Helpers;
using Common.Providers;
using RMAZOR.Models;

namespace RMAZOR.Views.Common
{
    public abstract class ViewMazeGroundBase : InitBase, IOnLevelStageChanged
    {
        private readonly   IModelGame     m_Model;
        protected readonly IColorProvider ColorProvider;

        protected ViewMazeGroundBase(IModelGame _Model, IColorProvider _ColorProvider)
        {
            m_Model = _Model;
            ColorProvider = _ColorProvider;
        }

        public override void Init()
        {
            ColorProvider.ColorThemeChanged += OnColorThemeChanged;
            base.Init();
        }

        public virtual void OnLevelStageChanged(LevelStageArgs _Args) { }
        
        private void OnColorThemeChanged(EColorTheme _Theme)
        {
            SetColorsOnNewLevelOrNewTheme(m_Model.LevelStaging.LevelIndex, _Theme);
        }

        protected virtual void SetColorsOnNewLevelOrNewTheme(long _LevelIndex, EColorTheme _Theme) { }
    }
}