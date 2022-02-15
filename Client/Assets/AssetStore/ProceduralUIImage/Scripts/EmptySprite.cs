using UnityEngine;
using System.Collections;

public static class EmptySprite
{
    private static Sprite _instance;

    ///<summary>
    /// Returns the instance of a (1 x 1) white Sprite
    /// </summary>	
    public static Sprite Get()
    {
        if (_instance == null)
        {
            _instance = Resources.Load<Sprite>("procedural_ui_image_default_sprite");
        }
        return _instance;
    }

    public static bool IsEmptySprite(Sprite _S)
    {
        return Get() == _S;
    }
}
