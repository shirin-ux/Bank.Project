using LoanGateway.Auth.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shahkar.Provider.Dto;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Shahkar.Provider
{
    public class ShahkarService(IHttpClientFactory http, IOptions<UidApiOptions> options, ILogger<ShahkarService> logger) : IShahkarService
    {
        private readonly IHttpClientFactory _http = http;
        private readonly IOptions<UidApiOptions> _options = options;
        private readonly ILogger<ShahkarService> _logger;
        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNamingPolicy = null,
            PropertyNameCaseInsensitive = true

        };
        public async Task<ShahkarMatchResponseDto> VerifyMobileOwnerAsync(string nationalId, string mobileNumber, CancellationToken cancellationToken = default)
        {
            var client = _http.CreateClient();
            var req = new 
            {
                NationalId = nationalId,
                MobileNumber = mobileNumber,
                RequestContext =new{
                    ApiInfo=new
                    {
                        BusinessId = _options.Value.BusinessId,
                        BusinessToken = _options.Value.BusinessToken
                    }
                }
                
            };
            var url = _options.Value.BaseUrl+"/api/inquiry/mobile/owner/v2";

            using var message = new HttpRequestMessage(HttpMethod.Post, url);

            var json = JsonSerializer.Serialize(req, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });


            message.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            using var response = await client.SendAsync(message, cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new LogicException(
                    $"UID mobile owner inquiry failed. StatusCode={(int)response.StatusCode}, Body={responseBody}");
            }

            var result = JsonSerializer.Deserialize<ShahkarMatchResponseDto>(
                responseBody, _json);

            if (result is null)
                throw new Exception("Empty or invalid response from UID mobile owner API.");

            return result;
        }
    }
}
