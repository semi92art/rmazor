using Newtonsoft.Json;
using UnityEngine;

namespace Common.CameraProviders.Camera_Effects_Props
{
    public class ColorGradingProps : ICameraEffectProps
    {
        [JsonProperty] public Color? Color            { get; set; }
        [JsonProperty] public int?   Hue              { get; set; }
        [JsonProperty] public float? Contrast         { get; set; }
        [JsonProperty] public float? Brightness       { get; set; }
        [JsonProperty] public float? Saturation       { get; set; }
        [JsonProperty] public float? Exposure         { get; set; }
        [JsonProperty] public float? Gamma            { get; set; }
        [JsonProperty] public float? Sharpness        { get; set; }
        [JsonProperty] public float? Blur             { get; set; }
        [JsonProperty] public Color? VignetteColor    { get; set; }
        [JsonProperty] public float? VignetteAmount   { get; set; }
        [JsonProperty] public float? VignetteSoftness { get; set; }
    }
}