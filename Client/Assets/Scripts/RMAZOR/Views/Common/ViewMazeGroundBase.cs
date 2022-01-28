using System.Linq;
using Common.Helpers;
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

        protected static float GetHForHSV(int _Group)
        {
            var values = new float[]
            {
                30,  // 1
                185, // 5
                55,  // 2
                225, // 6
                80,  // 3
                265, // 7
                140, // 4
                305, // 8
                // 330  // 9
            }.Select(_H => _H / 360f).ToArray();
            int idx = _Group % values.Length;
            return values[idx];
        }

        public virtual void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage != ELevelStage.Loaded)
                return;
            SetColorsOnTheme(_Args.LevelIndex, ColorProvider.CurrentTheme);
        }

        private void OnColorThemeChanged(EColorTheme _Theme)
        {
            SetColorsOnTheme(m_Model.LevelStaging.LevelIndex, _Theme);
        }
        
        protected abstract void GetHSV(int _LevelIndex, out float _H, out float _S, out float _V);
        protected abstract void SetColorsOnTheme(int _LevelIndex, EColorTheme _Theme);
    }
}