using mazing.common.Runtime;
using UnityEngine.Events;

namespace RMAZOR.Views.UI.Game_Logo
{
    public interface IViewUIGameLogo : IInit
    {
        event UnityAction Shown;
        bool              WasShown { get; }
        void              Show();
    }
}