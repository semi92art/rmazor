using UnityEngine.Events;

namespace Common.Managers.Advertising.AdBlocks
{
    public abstract class AdBase
    {
        protected UnityAction OnShown;
        protected UnityAction OnClicked;
    }
}