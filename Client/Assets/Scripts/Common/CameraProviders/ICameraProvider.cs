using UnityEngine;

namespace Common.CameraProviders
{
    public interface ICameraProvider
    {
        Camera MainCamera { get; }
        void   SetDofValue(float _Value);
        bool   DofEnabled { get; set; }
    }
}