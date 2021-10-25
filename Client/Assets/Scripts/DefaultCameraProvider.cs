using GameHelpers;
using LeTai.Asset.TranslucentImage;
using UnityEngine;

public interface ICameraProvider
{
    Camera MainCamera { get; }
}
    
public class DefaultCameraProvider : MonoBehaviour, ICameraProvider
{
    public Camera MainCamera { get; private set; }

    private void Awake()
    {
        MainCamera = Camera.main;
        PrefabUtilsEx.GetObject<ScalableBlurConfig>("views", "level_blur_config").Strength = 10f;
    }
}