using System.Collections.Generic;
using UnityEngine;

namespace Games.RazorMaze.Views.UI
{
    public interface IViewUIGetRenderers
    {
        List<Component> GetRenderers();
    }
}