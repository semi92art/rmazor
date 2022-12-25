using System;
using Common.Helpers;
using mazing.common.Runtime.CameraProviders.Camera_Effects_Props;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Helpers.Attributes;
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
    public class AdditionalColorPropsAdditionalInfo
    {
        [JsonProperty("A0")]               public bool   dark;
        [JsonProperty("A1"), Range(0, 1)]  public float  neonStreamColorCoefficient1;
        [JsonProperty("A2"), Range(0, 3)]  public float  swirlForPlanetColorCoefficient1;
        [JsonProperty("A3"), Range(0, 10)] public float  wormHoleColorCoefficient1;
        [JsonProperty("A4")]               public string backgroundName;
        [JsonProperty("A5")]               public string additionalBackgroundName;
    }
    
    [Serializable]
    public class AdditionalColorsProps
    {
        public Color main;
        public Color bacground1;
        public Color bacground2;
        public bool  inUse;
        
        [JsonProperty("F1")] public EBackAndFrontColorType pathItemFillType;
        [JsonProperty("F2")] public EBackAndFrontColorType pathBackgroundFillType;
        [JsonProperty("F3")] public EBackAndFrontColorType pathFillFillType;
        [JsonProperty("F4")] public EBackAndFrontColorType characterBorderFillType;
        [JsonProperty("F5")] public EBackAndFrontColorType uiBackgroundFillType;

        public BloomPropsArgs                      bloom;
        public AdditionalColorPropsAdditionalInfo additionalInfo;

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
    public class AdditionalColorsPropsSet : ReorderableArray<AdditionalColorsProps> { }
    
    [CreateAssetMenu(fileName = "back_and_front_colors_set", menuName = "Configs and Sets/Additional Colors Set", order = 0)]
    public class AdditionalColorsSetScriptableObject : ScriptableObject
    {
        [Header("Set"), Reorderable(paginate = true, pageSize = 50)]
        public AdditionalColorsPropsSet set;
    }
}