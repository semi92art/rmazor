using System;
using UnityEngine;

namespace Common.CameraProviders.Camera_Effects_Props
{
    public class BloomProps : ICameraEffectProps
    {
        public bool?  SetIterations { get; set; }
        public int?   Iterations    { get; set; }
        public float? Diffusion     { get; set; }
        public Color? Color         { get; set; }
        public float? Amount        { get; set; }
        public float? Threshold     { get; set; }
        public float? Softness      { get; set; }
    }

    
    [Serializable]
    public class BloomPropsArgs
    {
        public bool  enableBloom;
        public float diffusion;
        public Color color;
        public float amount;
        public float threshold;
        public float softness;

        public BloomProps ToBloomProps(out bool _EnableBloom)
        {
            _EnableBloom = enableBloom;
            var bloomProps = new BloomProps
            {
                Diffusion = diffusion,
                Color     = color,
                Amount    = amount,
                Threshold = threshold,
                Softness  = softness
            };
            return bloomProps;
        }
    }
}