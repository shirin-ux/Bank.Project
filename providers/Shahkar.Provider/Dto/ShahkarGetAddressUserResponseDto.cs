namespace Shahkar.Provider.Dto
{
    public class ShahkarGetAddressUserResponseDto
    {
        public string? postalCode { get; set; }
        public string? address { get; set; }
        public ResponseContext responseContext { get; set; } = null!;


        public class ResponseContext
        {
            public ResponseStatus status { get; set; } = null!;
            public string requestId { get; set; } = string.Empty;
            public string correlationId { get; set; } = string.Empty;
            public string navigationURI { get; set; } = string.Empty;
            public string nextStepToken { get; set; } = string.Empty;
            public string userSessionId { get; set; } = string.Empty;
            public Dictionary<string, object> custom { get; set; } = new();
        }

        public class ResponseStatus
        {
            public int code { get; set; }
            public string message { get; set; } = null!;
            public List<string> details { get; set; } = new();
        }
    }
}
