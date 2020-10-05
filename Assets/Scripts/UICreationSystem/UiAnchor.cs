using UnityEngine;

namespace UICreationSystem
{
    public struct UiAnchor
    {
        #region factory

        public static UiAnchor Create(Vector2 _AnchorMin, Vector2 _AnchorMax)
        {
            return Create(_AnchorMin.x, _AnchorMin.y, _AnchorMax.x, _AnchorMax.y);
        }

        public static UiAnchor Create(float _MinX, float _MinY, float _MaxX, float _MaxY)
        {
            return new UiAnchor(_MinX, _MinY, _MaxX, _MaxY);
        }

        #endregion

        #region properties

        public Vector2 Min { get; }
        public Vector2 Max { get; }

        #endregion

        #region constructors

        private UiAnchor(float _MinX, float _MinY, float _MaxX, float _MaxY)
        {
            Min = new Vector2(_MinX, _MinY);
            Max = new Vector2(_MaxX, _MaxY);
        }

        #endregion
    }
}
