using UnityEngine;

public static class Utility
{
    public static void CopyToClipboard(this string _Text)
    {
        TextEditor te = new TextEditor();
        te.text = _Text;
        te.SelectAll();
        te.Copy();
    }

    public static string ToStringAlt(this Vector2 _Value)
    {
        return $"({_Value.x.ToString("F2").Replace(',', '.')}f, " +
               $"{_Value.y.ToString("F2").Replace(',', '.')}f)";
    }
    

    public static Vector2 HalfOne => Vector2.one * .5f;
    public static Color Transparent => new Color(0f, 0f, 0f, 0f);
}
