using System;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Helpers.Attributes;
using RMAZOR.Views;
using UnityEngine;

namespace RMAZOR.Enitities
{
    [Serializable]
    public class LevelGenerationParamsSet : ReorderableArray<LevelGenerationParams> { }
    
    [CreateAssetMenu(fileName = "level_gen_params", menuName = "Configs and Sets/Level Generation Params Set")]
    public class LevelGenerationParamsSetScriptableObject : ScriptableObject
    {
        [Header("Set"), Reorderable(paginate = true, pageSize = 50)]
        public LevelGenerationParamsSet set;
    }
}