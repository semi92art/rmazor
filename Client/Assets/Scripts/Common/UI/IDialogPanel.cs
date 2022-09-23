using Common.Enums;
using Common.UI.DialogViewers;
using UnityEngine;
using UnityEngine.Events;

namespace Common.UI
{
    public delegate void ClosePanelAction(UnityAction _OnFinishClosing);
    
    public interface IDialogPanel
    {
        EDialogViewerType DialogViewerType   { get; }
        EUiCategory       Category           { get; }
        EAppearingState   AppearingState     { get; set; }
        RectTransform     PanelRectTransform { get; }
        Animator          Animator           { get; }
        
        void              LoadPanel(RectTransform _Container, ClosePanelAction _OnClose);
    }
    
    public class DialogPanelFake : IDialogPanel
    {
        public EDialogViewerType DialogViewerType   => default;
        public EUiCategory       Category           => EUiCategory.Fake;
        public EAppearingState   AppearingState     { get; set; } = EAppearingState.Dissapeared;
        public RectTransform     PanelRectTransform => null;
        public Animator          Animator           => null;
        
        public void              LoadPanel(RectTransform _Container, ClosePanelAction _OnClose) { }
    }
}