using Common.Exceptions;

namespace Common.Constants
{
    public static class DataFieldIds
    {
        // game field ids
        public const ushort Money = 1;
        public const ushort Level = 2;

        public static string GetDataFieldName(ushort _Id)
        {
            switch (_Id)
            {
                case Money:
                    return nameof(Money);
                case Level:
                    return nameof(Level);
                default:
                    throw new SwitchCaseNotImplementedException(_Id);
            }
        }
    }
}