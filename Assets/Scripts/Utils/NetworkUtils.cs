namespace Utils
{
    public static class NetworkUtils
    {
        public static bool IsPacketSuccess(long _ResponseCode)
        {
            return CommonUtils.IsInRange(_ResponseCode, 200, 299);
        }

        public static bool IsPacketFail(long _ResponseCode)
        {
            return CommonUtils.IsInRange(_ResponseCode, 400, 599) || _ResponseCode == 0;
        }
    }
}