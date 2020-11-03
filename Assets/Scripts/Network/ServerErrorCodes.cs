namespace Network
{
    public static class ServerErrorCodes
    {
        public const int AccountNotFoundByDeviceId = 200;
        public const int WrongLoginOrPassword = 201;
        public const int EntityNotFound = 202;
        //203
        public const int IncorrectRequest = 204;
        public const int DatabaseValidationFail = 205;
        public const int AccountNotFoundByAccountId = 206;
        //207
        public const int ProfileNotFound = 208;
        public const int ScoreNotFound = 209;
        public const int AccountWithThisNameAlreadyExist = 210;
        public const int AccountWithThisDeviceIdAlreadyExist = 211;
    }
}