using Common.Enums;
using Common.UI;
using UnityEngine;

namespace RMAZOR.UI.Panels
{
    public class FakeDialogPanel : IDialogPanel
    {
        public EUiCategory     Category       => EUiCategory.Fake;
        public bool            AllowMultiple  => false;
        public EAppearingState AppearingState { get; set; }
        public RectTransform   PanelObject    => null;
        public Animator        Animator       => null;

        public void LoadPanel()      { }
        public void OnDialogAppeared()   { }
        public void OnDialogDissapearing()   { }
        public void OnDialogStartAppearing() { }
    }
}