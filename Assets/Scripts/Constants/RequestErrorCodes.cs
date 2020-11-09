namespace Constants
{
    public class RequestErrorCodes
    {
        public const int AccontEntityNotFoundByDeviceId = 200;
        public const int WrongLoginOrPassword = 201;
        public const int EntityWithAccountIdAndGameIdNotFound = 202;
        //203 is free
        public const int RequestWasMadeIncorrectly = 204;
        public const int DatabaseValidationFailed = 205;
        public const int AccountEntityNotFoundByAccountId = 206;
        //207 is free
        public const int ProfileEntityNotFoundByAccountIdAndGameId = 208;
        public const int ScoreEntityNotFoundByAccountIdAndGameIdAndType = 209;
        public const int AccountEntityWithThisNameAlreadyExist = 210;
        public const int AccountEntityWithThisDeviceIdAlreadyExist = 211;
    }
}