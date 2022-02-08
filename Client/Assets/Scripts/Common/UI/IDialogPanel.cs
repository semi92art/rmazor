using Common.Enums;
using UnityEngine;

namespace Common.UI
{
    public interface IDialogPanel
    {
        EUiCategory     Category       { get; }
        bool            AllowMultiple  { get; }
        EAppearingState AppearingState { get; set; }
        RectTransform   PanelObject    { get; }
        void            LoadPanel();
        void            OnDialogShow();
        void            OnDialogHide();
        void            OnDialogEnable();
    }
}