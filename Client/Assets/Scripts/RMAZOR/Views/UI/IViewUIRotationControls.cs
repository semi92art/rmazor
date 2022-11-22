using System.Collections.Generic;
using RMAZOR.Models;
using UnityEngine;

namespace RMAZOR.Views.UI
{
    public interface IViewUIRotationControls :
        IOnLevelStageChanged,
        IInitViewUIItem,
        IViewUIGetRenderers
    {
        bool HasButtons { get; }
        void OnTutorialStarted(ETutorialType  _Type);
        void OnTutorialFinished(ETutorialType _Type);
    }

    public class ViewUIRotationControlsFake : IViewUIRotationControls
    {
        public bool                   HasButtons                                   => false;
        public void                   Init(Vector4                       _Offsets) { }
        public void                   OnLevelStageChanged(LevelStageArgs _Args)    { }
        public void                   OnTutorialStarted(ETutorialType    _Type)    { }
        public void                   OnTutorialFinished(ETutorialType   _Type)    { }
        public IEnumerable<Component> GetRenderers()                               => new List<Component>();
    }
}