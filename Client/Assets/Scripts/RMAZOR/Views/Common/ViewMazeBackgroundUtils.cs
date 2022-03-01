using System.Linq;

namespace RMAZOR.Views.Common
{
    public static class ViewMazeBackgroundUtils
    {
        public static float GetHForHSV(long _LevelIndex)
        {
            int group = RazorMazeUtils.GetGroupIndex(_LevelIndex);
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
                }.Select(_H => _H / 360f)
                .ToArray();
            int idx = (group - 1) % values.Length;
            return values[idx];
        }
    }
}