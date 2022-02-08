using System.Linq;
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
        
        public virtual void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage != ELevelStage.Loaded)
                return;
            SetColorsOnTheme(_Args.LevelIndex, ColorProvider.CurrentTheme);
        }
        
        protected static float GetHForHSV(int _Group)
        {
            var values = new float[]
            {
                0,
                // 30,
                185,
                // 55,
                225,
                80,
                265,
                140,
                305,
                220
                // 330 
            }.Select(_H => _H / 360f).ToArray();
            int idx = (_Group - 1) % values.Length;
            return values[idx];
        }

        private void OnColorThemeChanged(EColorTheme _Theme)
        {
            SetColorsOnTheme(m_Model.LevelStaging.LevelIndex, _Theme);
        }
        
        protected abstract void GetHSV(int _LevelIndex, out float _H, out float _S, out float _V);
        protected abstract void SetColorsOnTheme(int _LevelIndex, EColorTheme _Theme);
    }
}