using Bank.Mellat.Provider.Dtos;


namespace Bank.Mellat.Provider.Mapper
{
    public static class MellatErrorMapper
    {
        private static readonly HashSet<string> RetryableErrors = new()
    {
        "10002","10101","10102","10111","10112","10121","10181","10301","10171","10203","10303","10191",
        "10192","10162","10402","12100","12101","12106","12110","12105","10004","10008","10101","10111",
        "11005","12002","11002","11006","11015","12200","12203","12205","15003","16002","13103","13108",
        "16005","15000","14107","14108","15015","18002"
    };

        public static BankResponse<T> Map<T>(string errorCode, string errorMessage, T data = default)
        {
            return new BankResponse<T>
            {
                ErrorCode = errorCode,
                ErrorMessage = errorMessage,
                Data = data,
                ShouldRetry = RetryableErrors.Contains(errorCode)
            };
        }
    }
}
