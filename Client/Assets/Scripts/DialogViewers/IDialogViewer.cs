using UnityEngine;

namespace DialogViewers
{
    public interface IDialogViewerBase
    {
        void Init(RectTransform _Parent);
        RectTransform Container { get; }
    }
}