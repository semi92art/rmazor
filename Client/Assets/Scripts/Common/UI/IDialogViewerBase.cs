using System;
using UnityEngine;

namespace Common.UI
{
    public interface IDialogViewerBase
    {
        IDialogPanel      CurrentPanel                { get; }
        RectTransform     Container                   { get; }
        Func<bool> IsOtherDialogViewersShowing { get; set; }
        void              Init(RectTransform _Parent);
    }
}