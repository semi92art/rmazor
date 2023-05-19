using System.Runtime.CompilerServices;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using RMAZOR.Models;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;

namespace RMAZOR.Views.Characters.Head
{
    public abstract class ViewCharacterHeadWithBorderObjectAltBase : ViewCharacterHeadWithBorderObjectBase
    {
        protected ViewCharacterHeadWithBorderObjectAltBase(
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

        protected override void GetAngleAndVerticalScaleOnMoveFinished(
            EDirection _Direction,
            out float  _Angle,
            out float  _VertScale)
        {
            (_Angle, _VertScale) = _Direction switch
            {
                EDirection.Left  => (0f, 1f),
                EDirection.Right => (0f, 1f),
                EDirection.Down  => (90f, -1f),
                EDirection.Up    => (90f, -1f),
                _                => throw new SwitchExpressionException(_Direction)
            };
        }

        protected override void LookAtByOrientationOnMoveFinish(
            EDirection        _Direction,
            EMazeOrientation? _Orientation = null) { }
    }
}