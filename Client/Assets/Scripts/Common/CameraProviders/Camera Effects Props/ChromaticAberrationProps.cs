namespace Common.CameraProviders.Camera_Effects_Props
{
    public class ChromaticAberrationProps : ICameraEffectProps
    {
        public float? RedX              { get; set; }
        public float? RedY              { get; set; }
        public float? GreenX            { get; set; }
        public float? GreenY            { get; set; }
        public float? BlueX             { get; set; }
        public float? BlueY             { get; set; }
        public float? FishEyeDistortion { get; set; }
    }
}