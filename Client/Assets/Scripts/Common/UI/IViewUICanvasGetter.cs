using UnityEngine;

namespace Common.UI
{
    public interface IViewUICanvasGetter : IInit
    {
        Canvas GetCanvas();
    }
}