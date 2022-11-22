using System.Collections.Generic;
using UnityEngine;

namespace RMAZOR.Views.UI
{
    public interface IViewUIGetRenderers
    {
        IEnumerable<Component> GetRenderers();
    }
}