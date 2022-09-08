using Common.Enums;
using UnityEngine;
using UnityEngine.Events;

namespace Common.UI
{
    public delegate void ClosePanelAction(UnityAction _OnFinishClosing);
    
    public interface IDialogPanel
    {
        EDialogViewerType DialogViewerType   { get; }
        EUiCategory       Category           { get; }
        bool              AllowMultiple      { get; }
        EAppearingState   AppearingState     { get; set; }
        RectTransform     PanelRectTransform { get; }
        Animator          Animator           { get; }
        
        void              LoadPanel(RectTransform _Container, ClosePanelAction _OnClose);
    }
    
    public class DialogPanelFake : IDialogPanel
    {
        public EDialogViewerType DialogViewerType   => default;
        public EUiCategory       Category           => EUiCategory.Fake;
        public bool              AllowMultiple      => false;
        public EAppearingState   AppearingState     { get; set; } = EAppearingState.Dissapeared;
        public RectTransform     PanelRectTransform => null;
        public Animator          Animator           => null;
        
        public void              LoadPanel(RectTransform _Container, ClosePanelAction _OnClose) { }
    }
}