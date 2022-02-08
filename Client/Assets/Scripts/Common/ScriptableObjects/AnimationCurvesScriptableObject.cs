using System;
using System.Linq;
using Malee.List;
using UnityEngine;

namespace ScriptableObjects
{
    [Serializable]
    public class AnimCurveObj
    {
        public AnimationCurve item;
        public string         name;
    }
    
    [Serializable]
    public class AnimationCurvesArray : ReorderableArray<AnimCurveObj> { }

    [CreateAssetMenu(fileName = "animation_curves_set", menuName = "Animation Curves Set", order = 2)]
    public class AnimationCurvesScriptableObject : ScriptableObject
    {
        #region public fields
        
        [Header("Curves"), Reorderable(paginate = true, pageSize = 10)]
        public AnimationCurvesArray curves;

        #endregion

        #region api

        public AnimationCurve GetCurve(string _Name)
        {
            return curves.FirstOrDefault(_Curve => _Curve.name == _Name)?.item;
        }

        #endregion
    }
}