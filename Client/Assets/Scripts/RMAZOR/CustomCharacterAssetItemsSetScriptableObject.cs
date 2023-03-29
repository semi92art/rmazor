using System;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Helpers.Attributes;
using UnityEngine;

namespace RMAZOR
{
    [Serializable]
    public class CustomCharactersAssetItemBase
    {
        public string id;
        public bool   inUse;
        public int    gameMoneyCoast;
        public int    characterLevelToOpen;
    }
    
    [Serializable]
    public class CustomCharactersAssetItem : CustomCharactersAssetItemBase
    {
        public Sprite icon;
        public bool   overrideCharacterColor2;
        public Color  characterColor2;
    }
    
    [Serializable]
    public class CustomCharacterAssetItemsSet : ReorderableArray<CustomCharactersAssetItem> { }
    
    [CreateAssetMenu(fileName = "custom_characters_set", menuName = "Configs and Sets/Custom Characters Set", order = 0)]
    public class CustomCharacterAssetItemsSetScriptableObject : ScriptableObject
    {
        [Header("Set"), Reorderable(paginate = true, pageSize = 50)]
        public CustomCharacterAssetItemsSet set;
    }
}