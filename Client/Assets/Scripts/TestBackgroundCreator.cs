// using System;
// using System.Collections.Generic;
// using Common;
// using Common.Extensions;
// using Common.Utils;
// using RMAZOR.Views.Common.ViewMazeBackgroundTextureProviders;
// using Shapes;
// using UnityEngine;
//
// public enum EBackgroundType
// {
//     Linear,
//     Circled,
//     Waves,
//     Saw
// }
//
// [ExecuteInEditMode]
// public class TestBackgroundCreator : MonoBehaviour
// {
//     public EBackgroundType                 backgroundType;
//     public float                           lineWidth;
//     public float                           lineAngleDegrees;
//     public float                           circleRadiusOffset;
//     public EBackgroundCircleCenterPosition circleCenterPosition;
//     public Color                           color1, color2;
//     
//     private readonly List<Line> m_Lines = new List<Line>();
//     private readonly List<Disc> m_Discs = new List<Disc>();
//     
//     public void Create()
//     {
//         switch (backgroundType)
//         {
//             case EBackgroundType.Linear:
//                 CreateLinear();
//                 break;
//             case EBackgroundType.Circled:
//                 CreateCircled();
//                 break;
//             case EBackgroundType.Waves:
//                 CreateWaves();
//                 break;
//             case EBackgroundType.Saw:
//                 CreateSaw();
//                 break;
//             default:
//                 throw new ArgumentOutOfRangeException();
//         }
//     }
//
//     private void CreateLinear()
//     {
//         ClearAll();
//         var bounds = GraphicUtils.GetVisibleBounds();
//         float minX, maxX, minY, maxY;
//         (minX, maxX, minY, maxY) = (bounds.min.x, bounds.max.x, bounds.min.y, bounds.max.y);
//         float yDiff45Deg = maxX - minX;
//         float yPos = minY - yDiff45Deg + lineWidth * 0.5f;
//         while (yPos < maxY + yDiff45Deg)
//         {
//             var go = new GameObject("Line");
//             var line = go.AddComponent<Line>();
//             m_Lines.Add(line);
//             line.EndCaps = LineEndCap.Square;
//             yPos += lineWidth * 2f;
//         }
//     }
//
//     private void CreateCircled()
//     {
//         ClearAll();
//         var bounds = GraphicUtils.GetVisibleBounds();
//         float minX, maxX, minY, maxY;
//         (minX, maxX, minY, maxY) = (bounds.min.x, bounds.max.x, bounds.min.y, bounds.max.y);
//         float width = bounds.size.x;
//         float height = bounds.size.y;
//         float maxR = (circleCenterPosition == EBackgroundCircleCenterPosition.Center ? 
//             0.5f : 1f) * Mathf.Sqrt(width * width + height * height);
//         Vector2 center;
//         switch (circleCenterPosition)
//         {
//             case EBackgroundCircleCenterPosition.TopLeft:
//                 center = new Vector2(minX, maxY);
//                 break;
//             case EBackgroundCircleCenterPosition.TopRight:
//                 center = new Vector2(maxX, maxY);
//                 break;
//             case EBackgroundCircleCenterPosition.BottomLeft:
//                 center = new Vector2(minX, minY);
//                 break;
//             case EBackgroundCircleCenterPosition.BottomRight:
//                 center = new Vector2(maxX, minY);
//                 break;
//             case EBackgroundCircleCenterPosition.Center:
//                 center = bounds.center;
//                 break;
//             default:
//                 throw new ArgumentOutOfRangeException();
//         }
//
//         float r = circleRadiusOffset;
//         while (r < maxR)
//         {
//             var go = new GameObject("Circle");
//             var circle = go.AddComponent<Disc>();
//             m_Discs.Add(circle);
//             circle.Color = color1;
//             circle.transform.position = center;
//             circle.Radius = r;
//             circle.Type = DiscType.Ring;
//             circle.Thickness = circleRadiusOffset;
//             r += circleRadiusOffset * 2f;
//         }
//     }
//
//     private void CreateWaves()
//     {
//         
//     }
//
//     private void CreateSaw()
//     {
//         
//     }
//
//     private void Update()
//     {
//         Camera.main.backgroundColor = color2;
//         switch (backgroundType)
//         {
//             case EBackgroundType.Linear:
//                 UpdateLinear();
//                 break;
//             case EBackgroundType.Circled:
//                 UpdateCircled();
//                 break;
//             case EBackgroundType.Waves:
//                 UpdateWaves();
//                 break;
//             case EBackgroundType.Saw:
//                 UpdateSaw();
//                 break;
//             default:
//                 throw new ArgumentOutOfRangeException();
//         }
//     }
//
//     private void UpdateLinear()
//     {
//         var bounds = GraphicUtils.GetVisibleBounds();
//         float minX, maxX, minY;
//         (minX, maxX, minY, _) = (bounds.min.x, bounds.max.x, bounds.min.y, bounds.max.y);
//         
//         float yDiff45Deg = maxX - minX;
//         float yPos = minY - yDiff45Deg;
//         float tan = Mathf.Tan(lineAngleDegrees * Mathf.Deg2Rad);
//         float cos = Mathf.Cos(lineAngleDegrees * Mathf.Deg2Rad);
//         foreach (var line in m_Lines)
//         {
//             line.Color = color1;
//             line.Thickness = lineWidth * cos;
//             yPos += lineWidth * 2f;
//             line.Start = new Vector3(minX, yPos - yDiff45Deg * tan * 0.5f);
//             line.End = new Vector3(maxX, yPos + yDiff45Deg * tan * 0.5f);
//         }
//     }
//
//     private void UpdateCircled()
//     {
//         
//     }
//
//     private void UpdateWaves()
//     {
//         
//     }
//
//     private void UpdateSaw()
//     {
//         
//     }
//
//     private void ClearLinear()
//     {
//         foreach (var line in m_Lines)
//         {
//             if (line.IsNotNull())
//                 line.gameObject.DestroySafe();
//         }
//         m_Lines.Clear();
//     }
//
//     private void ClearCircled()
//     {
//         foreach (var disc in m_Discs)
//         {
//             if (disc.IsNotNull())
//                 disc.gameObject.DestroySafe();
//         }
//         m_Discs.Clear();
//     }
//
//     private void ClearAll()
//     {
//         ClearLinear();
//         ClearCircled();
//     }
// }

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