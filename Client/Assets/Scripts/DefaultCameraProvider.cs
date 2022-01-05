using GameHelpers;
using LeTai.Asset.TranslucentImage;
using StansAssets.Foundation.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public interface ICameraProvider
{
    Camera                 MainCamera        { get; }
    TranslucentImageSource TranslucentSource { get; }
    ScalableBlurConfig     BlurConfig        { get; }
}
    
public class DefaultCameraProvider : MonoBehaviour, ICameraProvider
{
    private IPrefabSetManager      PrefabSetManager { get; set; }
    
    [Inject]
    public void Inject(IPrefabSetManager _PrefabSetManager)
    {
        PrefabSetManager = _PrefabSetManager;
    }
    
    public Camera                 MainCamera        => Camera.main;
    public TranslucentImageSource TranslucentSource => MainCamera.GetComponent<TranslucentImageSource>();
    public ScalableBlurConfig     BlurConfig        { get; private set; }


    private void Awake()
    {
        BlurConfig = PrefabSetManager.GetObject<ScalableBlurConfig>(
            "views",
            "level_blur_config");
    }
}