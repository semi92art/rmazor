using LeTai.TrueShadow.PluginInterfaces;
using UnityEngine;
using UnityEngine.UI;

namespace LeTai.TrueShadow
{
public partial class TrueShadow
{
    ITrueShadowCasterMaterialProvider           casterMaterialProvider;
    ITrueShadowCasterMaterialPropertiesModifier casterMaterialPropertiesModifier;
    ITrueShadowCasterMeshModifier               casterMeshModifier;
    ITrueShadowCasterClearColorProvider         casterClearColorProvider;
    ITrueShadowRendererNormalMaterialProvider   rendererNormalMaterialProvider;
    ITrueShadowRendererMaterialModifier         rendererMaterialModifier;
    ITrueShadowRendererMeshModifier             rendererMeshModifier;

    void InitializePlugins()
    {
        casterMaterialProvider           = GetComponent<ITrueShadowCasterMaterialProvider>();
        casterMaterialPropertiesModifier = GetComponent<ITrueShadowCasterMaterialPropertiesModifier>();
        casterMeshModifier               = GetComponent<ITrueShadowCasterMeshModifier>();
        casterClearColorProvider         = GetComponent<ITrueShadowCasterClearColorProvider>();
        if (casterClearColorProvider != null)
            ColorBleedMode = ColorBleedMode.Plugin;

        rendererNormalMaterialProvider   = GetComponent<ITrueShadowRendererNormalMaterialProvider>();
        rendererMaterialModifier         = GetComponent<ITrueShadowRendererMaterialModifier>();
        rendererMeshModifier             = GetComponent<ITrueShadowRendererMeshModifier>();
    }

    public virtual Material GetShadowCastingMaterial()
    {
        return casterMaterialProvider != null
                   ? casterMaterialProvider.GetTrueShadowCasterMaterial()
                   : Graphic.material;
    }

    public virtual void ModifyShadowCastingMaterialProperties(MaterialPropertyBlock propertyBlock)
    {
        casterMaterialPropertiesModifier?.ModifyTrueShadowCasterMaterialProperties(propertyBlock);
    }

    public virtual void ModifyShadowCastingMesh(Mesh mesh)
    {
        casterMeshModifier?.ModifyTrueShadowCasterMesh(mesh);
    }

    public virtual Material GetShadowRenderingNormalMaterial()
    {
        return rendererNormalMaterialProvider != null
                   ? rendererNormalMaterialProvider.GetTrueShadowRendererNormalMaterial()
                   : Graphic.materialForRendering;
    }

    public virtual void ModifyShadowRendererMaterial(Material baseMaterial)
    {
        rendererMaterialModifier?.ModifyTrueShadowRendererMaterial(baseMaterial);
    }

    public virtual void ModifyShadowRendererMesh(VertexHelper vertexHelper)
    {
        rendererMeshModifier?.ModifyTrueShadowRenderMesh(vertexHelper);
    }
}
}
