using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.Contracts
{
    public interface ITokenProvider
    {
        Task<string> GetTokenAsync(CancellationToken ct = default);
    }
    public class TokenProvider : ITokenProvider
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _config;
        private string? _cachedToken;
        private DateTime _expiresAtUtc;

        public TokenProvider(IHttpClientFactory http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }


        public async Task<string> GetTokenAsync(CancellationToken ct = default)
        {
            var client = _http.CreateClient();
            var response = await client.PostAsync("https://gateway.local/connect/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = "investment-service",
                    ["client_secret"] = "***",
                    ["scope"] = "auth.internal"
                }),
                ct
            );

            response.EnsureSuccessStatusCode();
            var token = await response.Content.ReadFromJsonAsync<TokenResponse>(ct);
            return token!.access_token;
        }

        public sealed class TokenResponse
        {
            public string access_token { get; set; } = default!;
            public int expires_in { get; set; }
        }
    }
}
