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
        Animator        Animator       { get; }
        void            LoadPanel();
    }
    
    public class DialogPanelFake : IDialogPanel
    {
        public EUiCategory     Category       => EUiCategory.Fake;
        public bool            AllowMultiple  => false;
        public EAppearingState AppearingState { get; set; } = EAppearingState.Dissapeared;
        public RectTransform   PanelObject    => null;
        public Animator        Animator       => null;
        public void            LoadPanel()    { }
    }
}