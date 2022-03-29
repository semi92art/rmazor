using Common;
using Common.Extensions;
using Common.Utils;
using UnityEngine;

public class TestBackgroundCreator : MonoBehaviour
{
    public  GameObject     obj;
    public  MeshRenderer   rend;
    public  SpriteRenderer rend2;
    public int            stencilRef;

    public void SetBounds()
    {
        var tr = rend.transform;
        tr.position = Camera.main.transform.position.PlusZ(20f);
        var bds = GraphicUtils.GetVisibleBounds();
        tr.localScale = new Vector3(bds.size.x * 0.1f, 1f, bds.size.y * 0.1f);
    }

    public void GetUV()
    {
        var uv = Camera.main.WorldToScreenPoint(obj.transform.position);
        var uv2 = Camera.main.WorldToViewportPoint(obj.transform.position);
        Dbg.Log(uv + "; " + uv2);

        var mat = rend.sharedMaterial;
        mat.SetFloat("_CenterX", uv2.x);
        mat.SetFloat("_CenterY", uv2.y);
    }

    public void SetStencilRef()
    {
        rend2.sharedMaterial.SetFloat("_StencilRef", stencilRef);
    }
}