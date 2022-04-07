using System;
using Common.Helpers;
using Common.Helpers.Attributes;
using Newtonsoft.Json;
using UnityEngine;

namespace RMAZOR
{
    public enum EBackAndFrontColorType
    {
        Main = 0,
        Background1 = 1,
        Background2 = 2
    }
    
    [Serializable]
    public class BackAndFrontColorsSetItem
    {
        public                       Color                  main;
        public                       Color                  bacground1;
        public                       Color                  bacground2;
        [JsonProperty("F1")] public EBackAndFrontColorType pathItemFillType;
        [JsonProperty("F2")] public EBackAndFrontColorType pathBackgroundFillType;
        [JsonProperty("F3")] public EBackAndFrontColorType pathFillFillType;
        [JsonProperty("F4")] public EBackAndFrontColorType characterBorderFillType;

        public Color GetColor(EBackAndFrontColorType _Type)
        {
            return _Type switch
            {
                EBackAndFrontColorType.Main        => main,
                EBackAndFrontColorType.Background1 => bacground1,
                EBackAndFrontColorType.Background2 => bacground2,
                _                                  => Color.magenta
            };
        }
    }
    
    [Serializable]
    public class BackAndFrontColorsSet : ReorderableArray<BackAndFrontColorsSetItem> { }
    
    [CreateAssetMenu(fileName = "back_and_front_colors_set", menuName = "Configs and Sets/Back And Front Color Set", order = 0)]
    public class BackAndFrontColorsSetScriptableObject : ScriptableObject
    {
        [Header("Set"), Reorderable(paginate = true, pageSize = 50)]
        public BackAndFrontColorsSet set;
    }
}