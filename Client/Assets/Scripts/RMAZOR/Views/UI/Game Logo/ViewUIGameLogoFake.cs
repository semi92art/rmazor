using mazing.common.Runtime.Helpers;
using UnityEngine.Events;

namespace RMAZOR.Views.UI.Game_Logo
{
    public class ViewUIGameLogoFake : InitBase, IViewUIGameLogo
    {
        public event UnityAction Shown;
        public bool              WasShown => true;
        public void              Show()   { }
    }
}