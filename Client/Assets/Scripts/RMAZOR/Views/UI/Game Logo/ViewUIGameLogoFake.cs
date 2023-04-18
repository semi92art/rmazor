using mazing.common.Runtime.Helpers;
using UnityEngine.Events;

namespace RMAZOR.Views.UI.Game_Logo
{
    public class ViewUIGameLogoFake : InitBase, IViewUIGameLogo
    {
#pragma warning disable 0067
        public event UnityAction Shown;
#pragma warning restore 0067
        public bool              WasShown => true;
        public void              Show()   { }
    }
}