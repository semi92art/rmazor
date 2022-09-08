﻿using Common.Enums;
using Common.UI;
using UnityEngine;

namespace RMAZOR.UI.Panels
{
    public class FakeDialogPanel : IDialogPanel
    {
        public EDialogViewerType DialogViewerType   => default;
        public EUiCategory       Category           => EUiCategory.Fake;
        public bool              AllowMultiple      => false;
        public EAppearingState   AppearingState     { get; set; }
        public RectTransform     PanelRectTransform => null;
        public Animator          Animator           => null;
        
        public void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose) { }
    }
}