using System;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Helpers.Attributes;
using UnityEngine;

namespace RMAZOR
{
    [Serializable]
    public class CustomCharacterColorsSetAssetItem : CustomCharactersAssetItemBase
    {
        public Color  color1;
    }
    
    [Serializable]
    public class CustomCharacterColorsSetAssetItemsSet : ReorderableArray<CustomCharacterColorsSetAssetItem> { }
    
    [CreateAssetMenu(fileName = "custom_character_colors_set_set", menuName = "Configs and Sets/Custom Character Colors Set Set", order = 0)]
    public class CustomCharacterColorsSetAssetItemsSetScriptableObject : ScriptableObject
    {
        [Header("Set"), Reorderable(paginate = true, pageSize = 50)]
        public CustomCharacterColorsSetAssetItemsSet set;
    }
}