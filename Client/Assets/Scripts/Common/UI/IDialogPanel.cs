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
    
    public class DialogPanelFake : IDialogPanel
    {
        public EUiCategory     Category         => EUiCategory.Fake;
        public bool            AllowMultiple    => false;
        public EAppearingState AppearingState   { get; set; } = EAppearingState.Dissapeared;
        public RectTransform   PanelObject      => null;
        public void            LoadPanel()      { }
        public void            OnDialogShow()   { }
        public void            OnDialogHide()   { }
        public void            OnDialogEnable() { }
    }
}