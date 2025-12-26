namespace LoanGateway.Auth.Application
{
    public class AuthCookieOptions
    {
        public string RefreshTokenCookieName { get; init; } = "RefreshToken";
        public bool Secure { get; init; } = true;
        public string SameSite { get; init; } = "None"; 
        public string Path { get; init; } = "/";
    }
}
