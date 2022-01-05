using System;
using UI.Panels;
using UnityEngine;

namespace DialogViewers
{
    public interface IDialogViewerBase
    {
        IDialogPanel      CurrentPanel                { get; }
        RectTransform     Container                   { get; }
        Func<bool> IsOtherDialogViewersShowing { get; set; }
        void              Init(RectTransform _Parent);
    }
}