using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Views.UI.Game_Logo
{
    public class ViewUIGameLogoFake : IViewUIGameLogo
    {
        public void              Init(Vector4 _Offsets) { }
        public void              Show()                 { }
        public bool              WasShown               => true;
        public event UnityAction Shown;
        public void              Hide() { }
    }
}