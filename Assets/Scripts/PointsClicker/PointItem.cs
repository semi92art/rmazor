using System;
using Constants;
using UnityEngine;
using Shapes;

namespace PointsClicker
{
    public class PointItem : MonoBehaviour
    {
        #region serialized fields

        [SerializeField] private Disc background;
        [SerializeField] private Disc border;
        [SerializeField] private CapsuleCollider coll;
        [SerializeField] private Animator animator;

        #endregion
        
        #region api

        public float Radius
        {
            get => background.Radius;
            set => background.Radius = border.Radius = coll.radius = value;
        }

        public void AnimateAppearance()
        {
            animator.SetTrigger(AnimKeys.Anim);
        }
        
        #endregion
        
        #region engine methods

        private void Start()
        {
            background.Type = DiscType.Pie;
            background.DashSpace = DashSpace.Meters;
            border.DashSpace = DashSpace.Meters;
        }

        #endregion
    }
}