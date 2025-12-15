using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LoanService.Application.UseCase.Investment.Command.User;
using LoanGateway.Auth.Domain.Exceptions;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using Azure.Core;
using static System.Net.WebRequestMethods;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace LoanService.Application.Contracts
{
    public class UserRequest
    {
        public string nationalCode { get; set; }
    }
    public interface IUserApiClient
    {
        Task<UserCommand?> GetUserByNationalCodeAsync(UserRequest req, CancellationToken ct);
    }
    public sealed class UserApiClient : IUserApiClient
    {
        private readonly IHttpClientFactory _http ;
   
        private readonly ILogger<UserApiClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserApiClient(IHttpClientFactory http, IHttpContextAccessor httpContextAccessor, ILogger<UserApiClient> logger)
        {
            _http = http;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }
        private string? GetBearerToken()
        {
            var authHeader = _httpContextAccessor
                .HttpContext?
                .Request
                .Headers["Authorization"]
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(authHeader))
                return null;

            if (!authHeader.StartsWith("Bearer "))
                return null;

            return authHeader.Substring("Bearer ".Length);
        }
        public async Task<UserCommand?> GetUserByNationalCodeAsync(UserRequest req, CancellationToken ct)
        {
            try
            {
                var request = new
                {
                    nationalCode = req.nationalCode,
                };
                var token = GetBearerToken();
                var url = $"https://localhost:7262/api/Auth/get-user-by-nationalCode";

                using var message = new HttpRequestMessage(HttpMethod.Post, url);

                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                message.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var client = _http.CreateClient();
                using var response = await client.SendAsync(message, ct);

        
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("User not found: {NationalCode}", req.nationalCode);
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var user = await response.Content.ReadFromJsonAsync<UserCommand>(cancellationToken: ct);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Auth Service for NationalCode {NationalCode}", req.nationalCode);
                throw new LogicException ("خطا در ارتباط با Auth Service");
            }
        }
    }


}
