namespace Utils
{
    public static class NetworkUtils
    {
        public static bool IsPacketSuccess(long _ResponseCode)
        {
            return MathUtils.IsInRange(_ResponseCode, 200, 299);
        }
    }
}