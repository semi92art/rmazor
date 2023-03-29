using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;

namespace RMAZOR.Views.Characters.Head
{
    public abstract class ViewCharacterHeadWithBorderObjectBase : ViewCharacterHeadBase
    {
        private GameObject m_BorderObj;

        protected ViewCharacterHeadWithBorderObjectBase(
            ViewSettings                _ViewSettings,
            IColorProvider              _ColorProvider,
            IContainersGetter           _ContainersGetter,
            IPrefabSetManager           _PrefabSetManager,
            ICoordinateConverter        _CoordinateConverter,
            IRendererAppearTransitioner _AppearTransitioner,
            IViewInputCommandsProceeder _CommandsProceeder)
            : base(
                _ViewSettings, 
                _ColorProvider,
                _ContainersGetter,
                _PrefabSetManager,
                _CoordinateConverter,
                _AppearTransitioner, 
                _CommandsProceeder) { }

        protected override void InitPrefab()
        {
            base.InitPrefab();
            m_BorderObj = PrefabObj.GetContentItem("border");
        }
        
        protected override void UpdatePrefab()
        {
            float scale = GetScale();
            var localScale = Vector2.one * scale * RelativeLocalScale;
            m_BorderObj.transform.SetLocalScaleXY(localScale);
            base.UpdatePrefab();
        }
        
        protected override void LookAtByOrientationOnMoveStart(
            EDirection        _Direction,
            bool              _VerticalInverse,
            EMazeOrientation? _Orientation = null)
        {
            GetAngleAndHorizontalScaleOnMoveStart(_Direction, out float angle, out float horScale);
            var localRot = Quaternion.Euler(
                Vector3.forward * (angle + GetMazeAngleByCurrentOrientation(_Orientation)));
            m_BorderObj.transform.localRotation = localRot;
            float vertScale = _VerticalInverse ? -1f : 1f;
            float scale = CoordinateConverter.Scale;
            if (MathUtils.Equals(scale, 0f))
                scale = 1f;
            float scaleCoeff = scale * RelativeLocalScale;
            var localScale = scaleCoeff * new Vector3(horScale, vertScale, 1f);
            m_BorderObj.transform.localScale = localScale;
            base.LookAtByOrientationOnMoveStart(_Direction, _VerticalInverse, _Orientation);
        }
        
        protected override void LookAtByOrientationOnMoveFinish(
            EDirection        _Direction,
            EMazeOrientation? _Orientation = null)
        {
            GetAngleAndVerticalScaleOnMoveFinished(_Direction, out float angle, out float vertScale);
            var localRot = Quaternion.Euler(
                Vector3.forward * (angle + GetMazeAngleByCurrentOrientation(_Orientation)));
            m_BorderObj.transform.localRotation = localRot;
            float scaleCoeff = CoordinateConverter.Scale * RelativeLocalScale;
            var localScale = scaleCoeff * new Vector3(1f, vertScale, 1f);
            m_BorderObj.transform.localScale = localScale;
            base.LookAtByOrientationOnMoveFinish(_Direction, _Orientation);
        }
    }
}