using mazing.common.Runtime.Helpers;
using UnityEngine;

namespace RMAZOR
{
    public class CompanyLogoMonoBeh : MonoBehInitBase
    {
        #region serialized fields

        [SerializeField] private Canvas canvas;

        #endregion

        #region api

        public void EnableLogo(bool _Enable)
        {
            canvas.enabled = _Enable;
        }

        #endregion
    }
}