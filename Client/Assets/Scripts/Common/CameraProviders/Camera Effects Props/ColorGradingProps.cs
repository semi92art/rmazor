using UnityEngine;

namespace Common.CameraProviders.Camera_Effects_Props
{
    public class ColorGradingProps : ICameraEffectProps
    {
        public Color? Color            { get; set; }
        public int?   Hue              { get; set; }
        public float? Contrast         { get; set; }
        public float? Brightness       { get; set; }
        public float? Saturation       { get; set; }
        public float? Exposure         { get; set; }
        public float? Gamma            { get; set; }
        public float? Sharpness        { get; set; }
        public float? Blur             { get; set; }
        public Color? VignetteColor    { get; set; }
        public float? VignetteAmount   { get; set; }
        public float? VignetteSoftness { get; set; }
    }
}