using System.Collections.Generic;

namespace RMAZOR.Views.Common.ViewLevelStageSwitchers
{
    public interface IViewLevelStageSwitcherSingleStage
    {
        void SwitchLevelStage(Dictionary<string, object> _Args);
    }
}