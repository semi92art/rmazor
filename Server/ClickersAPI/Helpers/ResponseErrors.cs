namespace ClickersAPI.Helpers
{
    public static class ResponseErrors
    {
        public static ErrorResponse AccountDoesNotExist(string _Name) => new ErrorResponse(
            1, $"Account with name {_Name} does not exist");
        public static ErrorResponse LoginOrPasswordIncorrect => new ErrorResponse(
            2, "Login or password are incorrect");
        public static ErrorResponse EntityDoesNotExist => new ErrorResponse(
            3, "Entity with this AccountId and GameId does not exist");
        public static ErrorResponse RequestIsIncorrect => new ErrorResponse(
            4, "Request is incorrect");
        public static ErrorResponse DbValidationFail(object _ModelState) =>
            new ErrorResponse(5, "Validation of database failed", _ModelState);
        public static ErrorResponse AccountWithNameAlreadyExist => new ErrorResponse(
            6, "Account with this name already exist");
    }
}
