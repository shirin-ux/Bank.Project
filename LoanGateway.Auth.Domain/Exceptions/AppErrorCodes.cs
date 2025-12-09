namespace LoanGateway.Auth.Domain.Exceptions
{
    public static class AppErrorCodes
    {

        public const int UnknownError = 1000;
        public const int ValidationError = 1001;
        public const int LogicError = 1002;
        public const int NotFound = 1003;
        public const int Unauthorized = 1004;


        public const int ExternalServiceError = 2000;
        public const int ExternalServiceTimeout = 2001;

        public const int SmsMobileEmpty = 2100;
        public const int SmsMessageEmpty = 2101;
        public const int KavenegarApiError = 2110;
        public const int KavenegarHttpError = 2111;
        public const int KavenegarUnknownError = 2112;

        public const int SqlDuplicateKey = 3000;
        public const int SqlForeignKey = 3001;
        public const int SqlDeadlock = 3002;
        public const int SqlGenericError = 3099;
        public const int ShahkarMismatch = 30100;

    }
}

