using System.Linq;
using Constants;
using Extensions;
using GameHelpers;
using UnityEngine;
using Utils;

public class MainBackgroundRenderer : MonoBehaviour
{
    public static readonly int Color1 = Shader.PropertyToID("_Color1");
    public static readonly int Color2 = Shader.PropertyToID("_Color2");
    public RenderTexture Texture { get; set; }

    public MeshRenderer renderer;
    
    private Material m_Material;
    private bool m_Initialized;

    private void Start()
    {
        m_Material = renderer.sharedMaterial;
        m_Initialized = true;
        UpdateColors();
    }
    
    public static MainBackgroundRenderer Create()
    {
        string name = "Main Menu Background Renderer Camera";
        var cameras = GameObject.FindGameObjectsWithTag("TextureCamera");
        GameObject instance = cameras.FirstOrDefault(_Obj => _Obj.name == name);
        if (instance == null)
        {
            instance = PrefabUtilsEx.InitPrefab(
                null,
                "main_menu",
                "background_render_camera"
            );
            instance.transform.position = instance.transform.position.SetX(-100);
        }
        var result = instance.GetComponent<MainBackgroundRenderer>();
        result.Texture = result.gameObject.GetCompItem<Camera>("camera").targetTexture;
        return result;
    }

    public void UpdateColors()
    {
        if (!m_Initialized) return;
        var color1 = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiMainMenuBackgroundColor1);
        var color2 = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiMainMenuBackgroundColor2);
        m_Material.SetColor(Color1, color1);
        m_Material.SetColor(Color2, color2);
    }
}
