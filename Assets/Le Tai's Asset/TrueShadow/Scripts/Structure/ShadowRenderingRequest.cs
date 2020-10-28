using UnityEngine;
using UnityEngine.UI;

namespace LeTai.TrueShadow
{
public struct ShadowRenderingRequest
{
    const int SIZE_HASH_STEP = 1;

    public readonly TrueShadow shadow;

    public Vector2 size;

    readonly int hash;

    public ShadowRenderingRequest(TrueShadow shadow)
    {
        this.shadow = shadow;

        size = shadow.RectTransform.rect.size;
        // size = ((Image) shadow.Graphic).sprite.rect.size;

        // Tiled type cannot be batched by similar size
        int dimensionHash = shadow.Graphic is Image image && image.type == Image.Type.Tiled
                                ? size.GetHashCode()
                                : HashUtils.CombineHashCodes(
                                    Mathf.CeilToInt(size.x / SIZE_HASH_STEP) * SIZE_HASH_STEP,
                                    Mathf.CeilToInt(size.y / SIZE_HASH_STEP) * SIZE_HASH_STEP
                                );

        hash = HashUtils.CombineHashCodes(
            Mathf.CeilToInt(shadow.Size * 100),
            dimensionHash,
            shadow.ContentHash
        );
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;

        return GetHashCode() == obj.GetHashCode();
    }

    public override int GetHashCode()
    {
        return hash;
    }
}
}
