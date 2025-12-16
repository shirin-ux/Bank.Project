using Azure.Core;
using LoanGateway.Auth.Domain.Exceptions;
using LoanService.Application.UseCase.Investment.Command.User;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace LoanService.Application.Contracts
{
    public class UserRequest
    {
        public string nationalCode { get; set; }

    }

    public class UserIdRequest
    {
        public string PostalCode { get; set; }
        public Guid UserId { get; set; }
    }
    public interface IUserApiClient
    {
        Task<UserCommand?> GetUserByNationalCodeAsync(UserRequest req, CancellationToken ct);
        Task<UserCommand?> GetUserByIdAsync(UserIdRequest req, CancellationToken ct);
        Task<UserCommand?> UpdateUserAsync(UserIdRequest req, CancellationToken ct);
    }
    public sealed class UserApiClient : IUserApiClient
    {
        private readonly IHttpClientFactory _http;
        private readonly ITokenProvider _tokenProvider;


        private readonly ILogger<UserApiClient> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserApiClient(IHttpClientFactory http, IHttpContextAccessor httpContextAccessor, ITokenProvider tokenProvider, ILogger<UserApiClient> logger)
        {
            _http = http;
            _logger = logger;
            _tokenProvider = tokenProvider;

        }

        public async Task<UserCommand?> GetUserByIdAsync(UserIdRequest req, CancellationToken ct)
        {

            try
            {
                var token = await _tokenProvider.GetTokenAsync(ct);
               // var url = "https://gateway.khanoumi.local/auth/internal/users/exists";
                var url = $"https://192.168.87.12:3000/api/Auth/exists/{req.UserId}";

                using var message = new HttpRequestMessage(HttpMethod.Get, url);
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                message.Content = JsonContent.Create(new
                {
                    userId = req.UserId
                });

                var client = _http.CreateClient();
                using var response = await client.SendAsync(message, ct);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException("Unauthorized access to Auth Service");

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<UserCommand>(cancellationToken: ct);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error calling Auth Service for {UserId}", req.UserId);
                throw;
            }
        }

        public async Task<UserCommand?> GetUserByNationalCodeAsync(UserRequest req, CancellationToken ct)
        {
   
            try
            {
           
                var request = new
                {
                    nationalCode = req.nationalCode,
                };

                var url = $"https://192.168.87.12:3000/api/Auth/get-user-by-nationalCode";

                using var message = new HttpRequestMessage(HttpMethod.Post, url);

                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer");
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
                return new UserCommand
                {
                    NationalCode = user.NationalCode,
                    BirthDate = user.BirthDate,
                    MobileNumber = user.MobileNumber,
                    PostalCode = user.PostalCode,
                    UserId = user.UserId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Auth Service for NationalCode {NationalCode}", req.nationalCode);
                throw new LogicException("خطا در ارتباط با Auth Service");
            }
        }

        public async Task<UserCommand?> UpdateUserAsync(UserIdRequest req, CancellationToken ct)
        {
            try
            {
                var request = new
                {
                    UserId = req.UserId,
                    PostalCode = req.PostalCode
                };
                //var token = GetBearerToken();
                var url = $"https://192.168.87.12:3000/api/Auth/update-postalcode";

                using var message = new HttpRequestMessage(HttpMethod.Post, url);

                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer");
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
                    _logger.LogWarning("User not found: {PostalCode}", req.PostalCode);
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var user = await response.Content.ReadFromJsonAsync<UserCommand>(cancellationToken: ct);
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("Auth Service returned {StatusCode}: {Content}", response.StatusCode, content);
                }
                return new UserCommand
                {
                    NationalCode = user.NationalCode,
                    BirthDate = user.BirthDate,
                    MobileNumber = user.MobileNumber,
                    PostalCode = user.PostalCode,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Auth Service for PostalCode {PostalCode}", req.PostalCode);
                throw new LogicException("خطا در ارتباط با Auth Service");
            }
        }
    }


}
