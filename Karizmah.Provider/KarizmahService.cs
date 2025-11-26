using Karizmah.Provider.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Karizmah.Provider;

public class KarizmahService(IHttpClientFactory http, IOptions<KarizmahInvestmentOptions> options) : IKarizmahService
{
    private readonly IHttpClientFactory _http = http;
    private readonly IOptions<KarizmahInvestmentOptions> _options = options;
    private readonly ILogger<KarizmahService> _logger;
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = null,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

    };
    public async Task<BaseResponse<KarizmahCreatePolicyWithoutInitialPaymentResponseDto>> CreatePolicyWithoutInitialPaymentAsync(KarizmahCreatePolicyWithoutInitialPaymentRequestDto req, CancellationToken ct)
    {


        var token = await GetAccessTokenAsync(ct);

        var url = _options.Value.BaseAddress + _options.Value.DirectEndpoint;

        using var message = new HttpRequestMessage(HttpMethod.Post, url);

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        message.Headers.Add("agentId", _options.Value.agentId);

        message.Headers.Accept.Clear();

        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
        var traceId = await GenerateTraceIdAsync(ct);
        var request = new
        {
            address = req.address,
            birthDate = req.birthDate,
            nationalCode = req.nationalCode,
            age = req.age,
            postalCode = req.postalCode,
            coefficient = req.coefficient,
            coverageAliasName = req.coverageAliasName,
            description = req.description,
            traceId = traceId.data.traceId
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        message.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var client = _http.CreateClient("KarizmahApi");
        using var response = await client.SendAsync(message, ct);


        var body = await response.Content.ReadAsStringAsync(ct);

        var bankResponse = JsonSerializer.Deserialize<BaseResponse<KarizmahCreatePolicyWithoutInitialPaymentResponseDto>>(body);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Code={(int)response.StatusCode}, Message={body}");
        }


        if (bankResponse is null)
            throw new InvalidOperationException("پاسخی از بانک دریافت نشد");

        if (!bankResponse.isSuccess)
        {
            return new BaseResponse<KarizmahCreatePolicyWithoutInitialPaymentResponseDto>
            {
                isSuccess = false,
                errorMessages = bankResponse.errorMessages ?? new(),
                data = null!
            };
        }

        var dto = new KarizmahCreatePolicyWithoutInitialPaymentResponseDto
        {
            traceId = bankResponse.data.traceId,
            policyId = bankResponse.data.policyId,
            isRepeated = bankResponse.data.isRepeated
        };
        return new BaseResponse<KarizmahCreatePolicyWithoutInitialPaymentResponseDto>
        {
            isSuccess = true,
            errorMessages = new List<ErrorMessage>(),
            data = dto
        };

    }

    public async Task<BaseResponse<KarizmahTraceIdResponse>> GenerateTraceIdAsync(CancellationToken ct)
    {
        var client = _http.CreateClient("KarizmahApi");

        var token = await GetAccessTokenAsync(ct);

        var url = _options.Value.BaseAddress + _options.Value.TraceIdEndpoint;

        using var message = new HttpRequestMessage(HttpMethod.Get, url);

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        message.Headers.Add("x-agent-id", _options.Value.agentId);

        message.Headers.Accept.Clear();

        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

        using var response = await client.SendAsync(message, ct);

        var body = await response.Content.ReadAsStringAsync(ct);
        var bankResponse = JsonSerializer.Deserialize<BaseResponse<KarizmahTraceIdResponse>>(body);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Code={(int)response.StatusCode}, Message={body}");
        }


        if (bankResponse is null)
            throw new InvalidOperationException("پاسخی از بانک دریافت نشد");

        if (!bankResponse.isSuccess)
        {
            return new BaseResponse<KarizmahTraceIdResponse>
            {
                isSuccess = false,
                errorMessages = bankResponse.errorMessages ?? new(),
                data = null!
            };
        }

        var dto = new KarizmahTraceIdResponse
        {
            traceId = bankResponse.data.traceId
        };
        return new BaseResponse<KarizmahTraceIdResponse>
        {
            isSuccess = true,
            errorMessages = new List<ErrorMessage>(),
            data = dto
        };

    }

    public async Task<KarizmaTokenResponse> GetAccessTokenAsync(CancellationToken ct)
    {
        var client = _http.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, _options.Value.BaseUrlToken)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
          {
              { "grant_type", _options.Value.GrantType },
              { "client_id", _options.Value.ClientId},
              { "client_secret", _options.Value.ClientSecret },
              { "username",_options.Value.UserName},
              { "password",_options.Value.Password }
          })
        };



        var resp = await client.SendAsync(request, ct);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<KarizmaTokenResponse>(json);
        return new KarizmaTokenResponse
        {
            AccessToken = tokenResponse.AccessToken
        };
    }
    //درگاه
    public async Task<BaseResponse<KarizmahOrderBuyResponseDto>> BuyOrderAsync(KarizmahOrderBuyRequestDto req, CancellationToken ct)
    {
        var traceId = await GenerateTraceIdAsync(ct);
        var token = await GetAccessTokenAsync(ct);

        var url = _options.Value.BaseAddress + _options.Value.BuyEndpoint;

        using var message = new HttpRequestMessage(HttpMethod.Post, url);

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        message.Headers.Add("agentId", _options.Value.agentId);

        message.Headers.Accept.Clear();

        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

        var request = new
        {
            address = req.address,
            birthDate = req.birthDate,
            nationalCode = req.nationalCode,
            age = req.age,
            utm = req.utm,
            coefficient = req.coefficient,
            coverageAliasName = req.coverageAliasName,
            description = req.description,
            amount = req.amount,
            callbackUrl =_options.Value.CallbackBaseUrl,
            phoneNumber = req.phoneNumber,
            traceId = traceId.data.traceId
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        message.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var client = _http.CreateClient("KarizmahApi");
        using var response = await client.SendAsync(message, ct);


        var body = await response.Content.ReadAsStringAsync(ct);

        var bankResponse = JsonSerializer.Deserialize<BaseResponse<KarizmahOrderBuyResponseDto>>(body);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Code={(int)response.StatusCode}, Message={body}");
        }


        if (bankResponse is null)
            throw new InvalidOperationException("پاسخی از بانک دریافت نشد");

        if (!bankResponse.isSuccess)
        {
            return new BaseResponse<KarizmahOrderBuyResponseDto>
            {
                isSuccess = false,
                errorMessages = bankResponse.errorMessages ?? new(),
                data = null!
            };
        }

        var dto = new KarizmahOrderBuyResponseDto { };
        return new BaseResponse<KarizmahOrderBuyResponseDto>
        {
            isSuccess = true,
            errorMessages = new List<ErrorMessage>(),
            data = dto
        };
    }

    public async Task<BaseResponse<KarizmahIncreaseCapitalDirectResponseDto>> IncreaseCapitalDirectAsync(KarizmahIncreaseCapitalDirectRequestDto req, CancellationToken ct)
    {
        var traceId = await GenerateTraceIdAsync(ct);
        var token = await GetAccessTokenAsync(ct);

        var url = _options.Value.BaseAddress + _options.Value.IncreaseCapitalDirectEndpoint;

        using var message = new HttpRequestMessage(HttpMethod.Post, url);

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        message.Headers.Add("agentId", _options.Value.agentId);

        message.Headers.Accept.Clear();

        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

        var request = new
        {
            rceiptDate = req.rceiptDate,
            receiptNumber = req.receiptNumber,
            description = req.description,
            amount = req.amount,
            policyId = req.policyId,
            traceId = traceId.data.traceId
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        message.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var client = _http.CreateClient("KarizmahApi");
        using var response = await client.SendAsync(message, ct);


        var body = await response.Content.ReadAsStringAsync(ct);

        var bankResponse = JsonSerializer.Deserialize<BaseResponse<KarizmahIncreaseCapitalDirectResponseDto>>(body);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Code={(int)response.StatusCode}, Message={body}");
        }


        if (bankResponse is null)
            throw new InvalidOperationException("پاسخی از بانک دریافت نشد");

        if (!bankResponse.isSuccess)
        {
            return new BaseResponse<KarizmahIncreaseCapitalDirectResponseDto>
            {
                isSuccess = false,
                errorMessages = bankResponse.errorMessages ?? new(),
                data = null!
            };
        }

        var dto = new KarizmahIncreaseCapitalDirectResponseDto
        {
            isRepeated = bankResponse.data.isRepeated,
            traceId = bankResponse.data.traceId,
            Id = bankResponse.data.Id
        };
        return new BaseResponse<KarizmahIncreaseCapitalDirectResponseDto>
        {
            isSuccess = true,
            errorMessages = new List<ErrorMessage>(),
            data = dto
        };
    }
    //درگاه
    public async Task<BaseResponse<KarizmahIncreaseResponseDto>> IncreaseAsync(KarizmahIncreaseRequestDto req, CancellationToken ct)
    {
        var traceId = await GenerateTraceIdAsync(ct);
        var token = await GetAccessTokenAsync(ct);

        var url = _options.Value.BaseAddress + _options.Value.IncreaseCapitalDirectEndpoint;

        using var message = new HttpRequestMessage(HttpMethod.Post, url);

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        message.Headers.Add("agentId", _options.Value.agentId);

        message.Headers.Accept.Clear();

        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

        var request = new
        {
            callbackUrl =_options.Value.CallbackBaseUrl,
            description = req.description,
            amount = req.amount,
            policyId = req.policyId,
            traceId = traceId.data.traceId
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        message.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var client = _http.CreateClient("KarizmahApi");
        using var response = await client.SendAsync(message, ct);


        var body = await response.Content.ReadAsStringAsync(ct);

        var bankResponse = JsonSerializer.Deserialize<BaseResponse<KarizmahIncreaseResponseDto>>(body);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Code={(int)response.StatusCode}, Message={body}");
        }


        if (bankResponse is null)
            throw new InvalidOperationException("پاسخی از بانک دریافت نشد");

        if (!bankResponse.isSuccess)
        {
            return new BaseResponse<KarizmahIncreaseResponseDto>
            {
                isSuccess = false,
                errorMessages = bankResponse.errorMessages ?? new(),
                data = null!
            };
        }

        var dto = new KarizmahIncreaseResponseDto
        {
            Id = bankResponse.data.Id,
            traceId = bankResponse.data.traceId,
            isRepeated = bankResponse.data.isRepeated
        };
        return new BaseResponse<KarizmahIncreaseResponseDto>
        {
            isSuccess = true,
            errorMessages = new List<ErrorMessage>(),
            data = dto
        };
    }

    public async Task<BaseResponse<KarizmahDecreaseDirectResponseDto>> DecreaseDirectAsync(KarizmahDecreaseDirectRequestDto req, CancellationToken ct)
    {
        var traceId = await GenerateTraceIdAsync(ct);
        var token = await GetAccessTokenAsync(ct);

        var url = _options.Value.BaseAddress + _options.Value.DecreaseDirectEndpoint;

        using var message = new HttpRequestMessage(HttpMethod.Post, url);

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        message.Headers.Add("agentId", _options.Value.agentId);

        message.Headers.Accept.Clear();

        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

        var request = new
        {
            bankAccountNumber = req.bankAccountNumber,
            description = req.description,
            phoneNumber = req.phoneNumber,
            amount = req.amount,
            policyId = req.policyId,
            traceId = traceId.data.traceId
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        message.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var client = _http.CreateClient("KarizmahApi");
        using var response = await client.SendAsync(message, ct);


        var body = await response.Content.ReadAsStringAsync(ct);

        var bankResponse = JsonSerializer.Deserialize<BaseResponse<KarizmahDecreaseDirectResponseDto>>(body);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Code={(int)response.StatusCode}, Message={body}");
        }


        if (bankResponse is null)
            throw new InvalidOperationException("پاسخی از بانک دریافت نشد");

        if (!bankResponse.isSuccess)
        {
            return new BaseResponse<KarizmahDecreaseDirectResponseDto>
            {
                isSuccess = false,
                errorMessages = bankResponse.errorMessages ?? new(),
                data = null!
            };
        }

        var dto = new KarizmahDecreaseDirectResponseDto
        {
            isRepeated = bankResponse.data.isRepeated,
            Id = bankResponse.data.Id,
            traceId = bankResponse.data.traceId
        };
        return new BaseResponse<KarizmahDecreaseDirectResponseDto>
        {
            isSuccess = true,
            errorMessages = new List<ErrorMessage>(),
            data = dto
        };
    }

    public async Task<BaseResponse<KarizmahDecreaseVerifyResponseDto>> DecreaseVerifyAsync(KarizmahDecreaseVerifyRequestDto req, CancellationToken ct)
    {
        var token = await GetAccessTokenAsync(ct);

        var url = _options.Value.BaseAddress + _options.Value.DecreaseVerifyEndpoint;

        using var message = new HttpRequestMessage(HttpMethod.Post, url);

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        var request = new
        {
            orderId = req.orderId,
            otp = req.otp,

        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        message.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var client = _http.CreateClient("KarizmahApi");
        using var response = await client.SendAsync(message, ct);


        var body = await response.Content.ReadAsStringAsync(ct);

        var bankResponse = JsonSerializer.Deserialize<BaseResponse<KarizmahDecreaseVerifyResponseDto>>(body);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Code={(int)response.StatusCode}, Message={body}");
        }


        if (bankResponse is null)
            throw new InvalidOperationException("پاسخی از بانک دریافت نشد");

        if (!bankResponse.isSuccess)
        {
            return new BaseResponse<KarizmahDecreaseVerifyResponseDto>
            {
                isSuccess = false,
                errorMessages = bankResponse.errorMessages ?? new(),
                data = null!
            };
        }
        return new BaseResponse<KarizmahDecreaseVerifyResponseDto>
        {
            isSuccess = true,
            errorMessages = new List<ErrorMessage>(),

        };
    }
}
