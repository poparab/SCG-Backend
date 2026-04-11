namespace SCG.Application.Abstractions;

public static class ErrorCodes
{
    public static class Auth
    {
        public const string InvalidCredentials = "AUTH_INVALID_CREDENTIALS";
        public const string AccountPendingApproval = "AUTH_ACCOUNT_PENDING";
        public const string InvalidRefreshToken = "AUTH_INVALID_REFRESH_TOKEN";
        public const string UserNotFound = "AUTH_USER_NOT_FOUND";
    }

    public static class Agency
    {
        public const string NotFound = "AGENCY_NOT_FOUND";
        public const string EmailAlreadyExists = "AGENCY_EMAIL_EXISTS";
        public const string AlreadyApproved = "AGENCY_ALREADY_APPROVED";
        public const string InvalidStatus = "AGENCY_INVALID_STATUS";
    }

    public static class Wallet
    {
        public const string NotFound = "WALLET_NOT_FOUND";
        public const string InsufficientBalance = "WALLET_INSUFFICIENT_BALANCE";
        public const string InvalidAmount = "WALLET_INVALID_AMOUNT";
    }

    public static class Batch
    {
        public const string NotFound = "BATCH_NOT_FOUND";
        public const string EmptyBatch = "BATCH_EMPTY";
        public const string InvalidStatus = "BATCH_INVALID_STATUS";
        public const string TravelerNotFound = "BATCH_TRAVELER_NOT_FOUND";
    }

    public static class Inquiry
    {
        public const string NotFound = "INQUIRY_NOT_FOUND";
        public const string InvalidStatus = "INQUIRY_INVALID_STATUS";
        public const string PricingNotFound = "INQUIRY_PRICING_NOT_FOUND";
    }

    public static class Nationality
    {
        public const string NotFound = "NATIONALITY_NOT_FOUND";
        public const string CodeAlreadyExists = "NATIONALITY_CODE_EXISTS";
        public const string InvalidFee = "NATIONALITY_INVALID_FEE";
    }

    public static class Validation
    {
        public const string InvalidInput = "VALIDATION_INVALID_INPUT";
    }
}
