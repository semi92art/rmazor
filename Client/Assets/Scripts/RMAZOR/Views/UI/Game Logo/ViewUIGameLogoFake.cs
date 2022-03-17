using UnityEngine;

namespace RMAZOR.Views.UI.Game_Logo
{
    public class ViewUIGameLogoFake : IViewUIGameLogo
    {
        public void Init(Vector4 _Offsets) { }
        public void Show()                 { }
        public bool Shown                  => true;
        public void Hide()                 { }
    }
}