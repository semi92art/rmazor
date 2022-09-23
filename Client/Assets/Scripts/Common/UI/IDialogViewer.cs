using System;
using UnityEngine;
using UnityEngine.Events;

namespace Common.UI
{
    public interface IDialogViewer : IInit
    {
        IDialogPanel      CurrentPanel   { get; }
        RectTransform     Container      { get; }
        
        Func<bool> OtherDialogViewersShowing { get; set; }
        void       Back(UnityAction  _OnFinish                      = null);
        void       Show(IDialogPanel _Panel, float _AnimationSpeed = 1f, bool _HidePrevious = true);
    }
}