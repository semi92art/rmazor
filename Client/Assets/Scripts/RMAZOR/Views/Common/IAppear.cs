using mazing.common.Runtime.Enums;

namespace RMAZOR.Views.Common
{
    public interface IAppear
    {
        void Appear(bool _Appear);
        EAppearingState AppearingState { get; }
    }
}