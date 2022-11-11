using Common.Enums;
using Common.UI;
using Common.UI.DialogViewers;
using UnityEngine;

namespace RMAZOR.UI.Panels
{
    public class FakeDialogPanel : IDialogPanel
    {
        public EDialogViewerType DialogViewerType   => default;
        public EAppearingState   AppearingState     { get; set; }
        public RectTransform     PanelRectTransform => null;
        public Animator          Animator           => null;
        
        public void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose) { }
    }
}