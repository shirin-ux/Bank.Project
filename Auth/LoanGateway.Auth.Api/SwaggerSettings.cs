namespace LoanGateway.Auth.Api
{
    public class SwaggerSettings
    {
        public bool Enabled { get; set; } = true;

        public string Title { get; set; } = "API";

        public string Version { get; set; } = "v1";
        public string RoutePrefix { get; set; } = "swagger";
    }
}
