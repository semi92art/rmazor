using Common;
using Common.Helpers;
using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using RMAZOR.Models;
using RMAZOR.Views.UI;
using UnityEngine;

namespace RMAZOR.Views.Common.ViewUILevelSkippers
{
    public interface IViewUILevelSkipper : IInit, IInitViewUIItem, IOnLevelStageChanged 
    {
        bool LevelSkipped { get; }
    }

    public class ViewUILevelSkipperFake : InitBase, IViewUILevelSkipper
    {
        public bool LevelSkipped                                 => false;
        public void Init(Vector4                       _Offsets) { }
        public void OnLevelStageChanged(LevelStageArgs _Args)    { }
    }
}