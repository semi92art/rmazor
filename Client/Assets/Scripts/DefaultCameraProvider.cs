using GameHelpers;
using LeTai.Asset.TranslucentImage;
using UnityEngine;
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
    
    public Camera                 MainCamera        { get; private set; }
    public TranslucentImageSource TranslucentSource { get; private set; }
    public ScalableBlurConfig     BlurConfig        { get; private set; }


    private void Awake()
    {
        MainCamera = Camera.main;
        TranslucentSource = MainCamera.GetComponent<TranslucentImageSource>();
        BlurConfig = PrefabSetManager.GetObject<ScalableBlurConfig>(
            "views",
            "level_blur_config");
    }
}