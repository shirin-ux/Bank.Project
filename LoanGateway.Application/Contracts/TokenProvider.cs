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
            if (_cachedToken != null && _expiresAtUtc > DateTime.UtcNow.AddMinutes(1))
                return  _cachedToken;


            var client = _http.CreateClient();

            var response = await client.PostAsync(
            _config["Auth:TokenUrl"],
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _config["Auth:ClientId"],
                ["client_secret"] = _config["Auth:ClientSecret"],
                ["scope"] = "auth.api"
            }),
            ct);

            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(ct);

            _cachedToken = tokenResponse!.access_token;
            _expiresAtUtc = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in);

            return _cachedToken;
        }

        public sealed class TokenResponse
        {
            public string access_token { get; set; } = default!;
            public int expires_in { get; set; }
        }
    }
}
