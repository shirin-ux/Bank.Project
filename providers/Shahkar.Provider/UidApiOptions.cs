namespace Shahkar.Provider
{
    public class UidApiOptions
    {

        public const string SectionName = "UidApi";

        public string BaseUrl { get; set; } 
        public string BusinessId { get; set; } = default!;
        public string BusinessToken { get; set; } = default!;
    }
}
