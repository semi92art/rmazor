using Common;
using mazing.common.Runtime;
using UnityEngine;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public interface IViewMazeGameLogoTextureProvider : IInit
    {
        void Activate(bool            _Active);
        void SetTransitionValue(float _Value, bool _Appear);
        void SetColor(Color           _Color);
    }
}