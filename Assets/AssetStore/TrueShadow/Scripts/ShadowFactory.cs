using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LeTai.TrueShadow
{
public class ShadowFactory
{
    private static ShadowFactory instance;
    public static  ShadowFactory Instance => instance ?? (instance = new ShadowFactory());

    readonly Dictionary<int, ShadowContainer> shadowCache =
        new Dictionary<int, ShadowContainer>();

    readonly CommandBuffer         cmd;
    readonly MaterialPropertyBlock materialProps;

    Material cutoutMaterial;

    Material CutoutMaterial =>
        cutoutMaterial ? cutoutMaterial : cutoutMaterial = new Material(Shader.Find("Hidden/ShadowCutout"));

    private ShadowFactory()
    {
        cmd           = new CommandBuffer {name = "Shadow Commands"};
        materialProps = new MaterialPropertyBlock();
        materialProps.SetVector(ShaderId.CLIP_RECT,
                                new Vector4(float.NegativeInfinity, float.NegativeInfinity,
                                            float.PositiveInfinity, float.PositiveInfinity));
        materialProps.SetInt(ShaderId.COLOR_MASK, (int) ColorWriteMask.All); // Render shadow even if mask hide graphic
    }

#if LETAI_TRUESHADOW_DEBUG
    RenderTexture debugTexture;
#endif

    // public int createdContainerCount;
    // public int releasedContainerCount;

    public void Get(ShadowRenderingRequest request, ref ShadowContainer container)
    {
        if (float.IsNaN(request.size.x) || request.size.x < 1 ||
            float.IsNaN(request.size.y) || request.size.y < 1)
        {
            ReleaseContainer(container);
            return;
        }

#if LETAI_TRUESHADOW_DEBUG
        RenderTexture.ReleaseTemporary(debugTexture);
        if (request.shadow.alwaysRender)
            debugTexture = RenderShadow(request);
#endif

        // Each request need a coresponding shadow texture
        // Texture may be shared by multiple elements
        // Texture are released when no longer used by any element
        // ShadowContainer keep track of texture and their usage


        int requestHash = request.GetHashCode();

        // Case: requester can keep the same texture
        if (container?.requestHash == requestHash)
            return;

        ReleaseContainer(container);

        if (shadowCache.TryGetValue(requestHash, out var existingContainer))
        {
            // Case: requester got texture from someone else
            existingContainer.RefCount++;
            container = existingContainer;
        }
        else
        {
            // Case: requester got new unique texture
            container = shadowCache[requestHash] = new ShadowContainer(RenderShadow(request), request);
            // Debug.Log($"Created new container for request\t{requestHash}\tTotal Created: {++createdContainerCount}\t Alive: {createdContainerCount - releasedContainerCount}");
        }
    }

    internal void ReleaseContainer(ShadowContainer container)
    {
        if (container == null)
            return;

        if (--container.RefCount > 0)
            return;

        RenderTexture.ReleaseTemporary(container.Texture);
        shadowCache.Remove(container.requestHash);

        // Debug.Log($"Released container for request\t{container.requestHash}\tTotal Released: {++releasedContainerCount}\t Alive: {createdContainerCount - releasedContainerCount}");
    }

    static readonly Rect UNIT_RECT      = new Rect(0, 0, 1, 1);
    static readonly int  IMPRINT_TEX_ID = Shader.PropertyToID("True Shadow Imprint Buffer");

    RenderTexture RenderShadow(ShadowRenderingRequest request)
    {
        // return GenColoredTexture(request.GetHashCode());

        cmd.Clear();
        cmd.BeginSample("TrueShadow:Capture");

        var padding   = Mathf.CeilToInt(request.shadow.Size);
        var tw        = Mathf.CeilToInt(request.size.x) + padding * 2;
        var th        = Mathf.CeilToInt(request.size.y) + padding * 2;
        var shadowTex = RenderTexture.GetTemporary(tw, th, 0, RenderTextureFormat.Default);
        cmd.GetTemporaryRT(IMPRINT_TEX_ID, tw, th, 0, FilterMode.Bilinear, RenderTextureFormat.Default);

        var texture = request.shadow.Content;
        if (texture)
            materialProps.SetTexture(ShaderId.MAIN_TEX, texture);
        else
            materialProps.SetTexture(ShaderId.MAIN_TEX, Texture2D.whiteTexture);

        cmd.SetRenderTarget(IMPRINT_TEX_ID);
        cmd.ClearRenderTarget(true, true, request.shadow.ClearColor);

        var imprintViewportSize = new Vector2(Mathf.Round(request.size.x), Mathf.Round(request.size.y));
        cmd.SetViewport(new Rect(padding, padding, imprintViewportSize.x, imprintViewportSize.y));
        var expansion = imprintViewportSize - request.size;
        var bounds    = request.shadow.RectTransform.rect;
        cmd.SetViewProjectionMatrices(
            Matrix4x4.identity,
            Matrix4x4.Ortho(bounds.min.x, bounds.max.x + expansion.x,
                            bounds.min.y, bounds.max.y + expansion.y,
                            -1, 1)
        );

        request.shadow.ModifyShadowCastingMesh(request.shadow.SpriteMesh);
        request.shadow.ModifyShadowCastingMaterialProperties(materialProps);
        cmd.DrawMesh(request.shadow.SpriteMesh,
                     Matrix4x4.identity,
                     request.shadow.GetShadowCastingMaterial(),
                     0, 0,
                     materialProps);
        cmd.EndSample("TrueShadow:Capture");


        cmd.BeginSample("TrueShadow:Cast");
        if (request.shadow.Size < 1e-2)
        {
            cmd.Blit(IMPRINT_TEX_ID, shadowTex);
        }
        else
        {
            request.shadow.blurProcessor.Blur(cmd, IMPRINT_TEX_ID, UNIT_RECT, shadowTex);
        }

        cmd.EndSample("TrueShadow:Cast");

        if (request.shadow.Cutout)
        {
            cmd.BeginSample("TrueShadow:Cutout");
            var offset = request.shadow.Offset;
            offset = request.shadow.transform.InverseTransformDirection(offset);
            CutoutMaterial.SetVector(ShaderId.OFFSET, new Vector2(offset.x / tw,
                                                                  offset.y / th));
            cmd.SetViewport(UNIT_RECT);
            cmd.Blit(IMPRINT_TEX_ID, shadowTex, CutoutMaterial);
            cmd.EndSample("TrueShadow:Cutout");
        }

        cmd.ReleaseTemporaryRT(IMPRINT_TEX_ID);

        Graphics.ExecuteCommandBuffer(cmd);

        return shadowTex;
    }

    RenderTexture GenColoredTexture(int hash)
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixels32(new[] {new Color32((byte) (hash >> 8), (byte) (hash >> 16), (byte) (hash >> 24), 255)});
        tex.Apply();

        var rt = RenderTexture.GetTemporary(1, 1);
        Graphics.Blit(tex, rt);

        return rt;
    }
}
}
