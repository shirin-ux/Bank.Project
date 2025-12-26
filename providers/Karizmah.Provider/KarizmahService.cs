using Common;
using Karizmah.Provider.Dtos;
using LoanService.Domain.Exceptions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Microsoft.Extensions;
namespace Karizmah.Provider;

public class KarizmahService(
    IHttpClientFactory http,
    IOptions<KarizmahInvestmentOptions> options) : IKarizmahService
{
    private readonly IHttpClientFactory _http = http;
    private readonly IOptions<KarizmahInvestmentOptions> _options = options;
    private readonly ILogger<KarizmahService> _logger;
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = null,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

    };

  public async Task<BaseResponse<KarizmahCreatePolicyWithoutInitialPaymentResponseDto>> CreatePolicyWithoutInitialPaymentAsync(
    KarizmahCreatePolicyWithoutInitialPaymentRequestDto req,
    CancellationToken ct)
    {
        try
        {
            var token = await GetAccessTokenAsync(ct);

            var url = _options.Value.BaseUrlApi + _options.Value.DirectEndpoint;

            using var message = new HttpRequestMessage(HttpMethod.Post, url);

            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            message.Headers.Add("x-agent-id", _options.Value.agentId);
            message.Headers.Accept.Clear();
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

            var traceId = await GenerateTraceIdAsync(ct);
            string formattedBirthDate= DateExtensions
                    .ConvertPersianToGregorian(req.birthDate)
                    .ToString("yyyy-MM-dd");

            var requestBody = new
            {
                planTypeAliasName = req.planTypeAliasName,
                birthDate = formattedBirthDate,
                nationalCode = req.nationalCode,
                age = req.age,
                postalCode = req.postalCode,
                coefficient = req.coefficient,
                coverageAliasName = req.coverageAliasName,
                traceId = traceId.data.traceId,
                address = req.address,
                description=req.description
            };

            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            message.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var client = _http.CreateClient();

            HttpResponseMessage response;
            string body = string.Empty;

            try
            {
                response = await client.SendAsync(message, ct);
                body = await response.Content.ReadAsStringAsync(ct);
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                throw new HttpRequestException("Timeout در ارتباط با سرویس خارجی Karizmah.", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException("خطا در ارتباط با سرویس خارجی Karizmah.", ex);
            }

            BaseResponse<KarizmahCreatePolicyWithoutInitialPaymentResponseDto>? bankResponse;

            try
            {
                bankResponse = JsonSerializer.Deserialize<BaseResponse<KarizmahCreatePolicyWithoutInitialPaymentResponseDto>>(body);
            }
            catch (JsonException ex)
            {
                throw new HttpRequestException(
                    $"پاسخ سرویس خارجی Karizmah قابل پردازش نیست. StatusCode={(int)response.StatusCode}, Body={body}",
                    ex);
            }

            if (bankResponse is null)
            {
                throw new HttpRequestException(
                    $"پاسخ سرویس خارجی Karizmah خالی یا نامعتبر است. StatusCode={(int)response.StatusCode}, Body={body}");
            }


            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                bankResponse.isSuccess = false;
                bankResponse.data = default!;
                return bankResponse;
            }


            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"خطای سرویس خارجی Karizmah. StatusCode={(int)response.StatusCode}, Body={body}");
            }


            if (!bankResponse.isSuccess || bankResponse.data is null)
            {
                bankResponse.isSuccess = false;
                bankResponse.data = default!;
                return bankResponse;
            }

            return new BaseResponse<KarizmahCreatePolicyWithoutInitialPaymentResponseDto>
            {
                isSuccess = true,
                errorMessages = bankResponse.errorMessages ?? new List<ErrorMessage>(),
                data = new KarizmahCreatePolicyWithoutInitialPaymentResponseDto
                {
                    traceId = bankResponse.data.traceId,
                    id = bankResponse.data.id,
                    isRepeated = bankResponse.data.isRepeated
                }
            };
        }
        catch (Exception ex)
        {

            throw;
        }
       
    }


    public async Task<BaseResponse<KarizmahOrderDto>> GetOrderBuyByIdAsync(Guid id, CancellationToken ct)
    {
        var token = await GetAccessTokenAsync(ct);


        var url = $"{_options.Value.BaseUrlApi}api/order/{id}";

        using var message = new HttpRequestMessage(HttpMethod.Get, url);

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        message.Headers.Add("x-agent-id", _options.Value.agentId);
        message.Headers.Accept.Clear();
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var client = _http.CreateClient();

        HttpResponseMessage response;
        string body = string.Empty;

        try
        {
            response = await client.SendAsync(message, ct);
            body = await response.Content.ReadAsStringAsync(ct);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new HttpRequestException("Timeout در ارتباط با سرویس خارجی Karizmah (GET /order).", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException("خطا در ارتباط با سرویس خارجی Karizmah (GET /order).", ex);
        }

        BaseResponse<KarizmahOrderDto>? bankResponse;

        try
        {
            bankResponse = JsonSerializer.Deserialize<BaseResponse<KarizmahOrderDto>>(body);
        }
        catch (JsonException ex)
        {
            throw new HttpRequestException(
                $"پاسخ سرویس خارجی Karizmah (GET /order) قابل پردازش نیست. StatusCode={(int)response.StatusCode}, Body={body}",
                ex);
        }

        if (bankResponse is null)
        {
            throw new HttpRequestException(
                $"پاسخ سرویس خارجی Karizmah (GET /order) خالی یا نامعتبر است. StatusCode={(int)response.StatusCode}, Body={body}");
        }


        if (!response.IsSuccessStatusCode)
        {

            throw new HttpRequestException(
                $"خطای سرویس خارجی Karizmah (GET /order). StatusCode={(int)response.StatusCode}, Body={body}");
        }

        if (!bankResponse.isSuccess || bankResponse.data is null)
        {
            var msg = bankResponse.errorMessages != null && bankResponse.errorMessages.Any()
                ? string.Join(" | ", bankResponse.errorMessages.Select(e => e.message))
                : "خطا در بازیابی وضعیت سفارش از سرویس Karizmah.";

            throw new InvalidOperationException(msg);
        }
        return new BaseResponse<KarizmahOrderDto>
        {
            isSuccess = true,
            errorMessages = bankResponse.errorMessages ?? new List<ErrorMessage>(),
            data = new KarizmahOrderDto
            {
                status = bankResponse.data.status,
                uliStatus = bankResponse.data.uliStatus,
                wealthPolicyId=bankResponse.data.wealthPolicyId
            }
        };

    }


    public async Task<BaseResponse<KarizmahTraceIdResponse>> GenerateTraceIdAsync(CancellationToken ct)
    {
        var client = _http.CreateClient();

        var token = await GetAccessTokenAsync(ct);

        var baseUri = new Uri(_options.Value.BaseUrlApi);
        var url = new Uri(baseUri, _options.Value.TraceIdEndpoint);

        using var message = new HttpRequestMessage(HttpMethod.Get, url);

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        message.Headers.Add("x-agent-id", _options.Value.agentId);

        message.Headers.Accept.Clear();

        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

        using var response = await client.SendAsync(message, ct);

        var body = await response.Content.ReadAsStringAsync(ct);
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var bankResponse = JsonSerializer.Deserialize<BaseResponse<long>>(body, jsonOptions);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Code={(int)response.StatusCode}, Message={body}");
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
            traceId = bankResponse.data
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
        try
        {
            var client = _http.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Post, _options.Value.BaseUrlToken);

            // بدنه فرم مثل Postman
            var form = new Dictionary<string, string>
    {
        { "grant_type", _options.Value.GrantType },
        { "client_id", _options.Value.ClientId },
        { "client_secret", _options.Value.ClientSecret }
    };
            request.Content = new FormUrlEncodedContent(form);


            var basicBytes = Encoding.ASCII.GetBytes($"{_options.Value.ClientId}:{_options.Value.ClientSecret}");
            var basicToken = Convert.ToBase64String(basicBytes);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicToken);


            var resp = await client.SendAsync(request, ct);
            var json = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Karizmah token endpoint failed. Status={StatusCode}, Body={Body}",
                    (int)resp.StatusCode, json);

                throw new HttpRequestException(
                    $"Karizmah token endpoint failed. Status={(int)resp.StatusCode}, Body={json}");
            }

            var tokenResponse = JsonSerializer.Deserialize<KarizmaTokenResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (tokenResponse is null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
                throw new InvalidOperationException("Invalid token response from Karizmah.");

            return tokenResponse;
        }
        catch (Exception ex)
        {

            throw;
        }

    }


    //درگاه
    public async Task<BaseResponse<KarizmahOrderBuyResponseDto>> BuyOrderAsync(KarizmahOrderBuyRequestDto req, CancellationToken ct)
    {
        var traceId = await GenerateTraceIdAsync(ct);
        var token = await GetAccessTokenChindxAsync(ct);

        var url = _options.Value.BuyEndpoint;

        using var message = new HttpRequestMessage(HttpMethod.Post, url);

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        message.Headers.Add("x-agent-id", _options.Value.agentId);

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
            callbackUrl = req.callbackUrl,
            phoneNumber = req.phoneNumber,
            traceId = traceId.data.traceId
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        message.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var client = _http.CreateClient();
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

        var dto = new KarizmahOrderBuyResponseDto
        {

            birthDate = req.birthDate,
            traceId = traceId.data.traceId,
            nationalCode = req.nationalCode,
            amount = req.amount,

        };
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

        var url = _options.Value.IncreaseCapitalDirectEndpoint;

        using var message = new HttpRequestMessage(HttpMethod.Post, url);

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        message.Headers.Add("x-agent-id", _options.Value.agentId);

        message.Headers.Accept.Clear();

        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

        // تبدیل receiptDate به فرمت مورد نیاز API کاریزما (yyyy-MM-dd)
        // اگر مقدار null یا default باشد، از تاریخ امروز استفاده می‌کنیم
        var receiptDateValue = req.rceiptDate;
        if (receiptDateValue == default(DateTime))
        {
            receiptDateValue = DateTime.UtcNow;
        }
        
        // فرمت: yyyy-MM-dd (مثل birthDate)
        var formattedReceiptDate = receiptDateValue.ToUniversalTime().ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        
        var request = new
        {
            receiptDate = req.rceiptDate.ToUniversalTime().ToString("O"),
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
        var token = await GetAccessTokenChindxAsync(ct);

        var url = _options.Value.IncreaseCapitalDirectEndpoint;

        using var message = new HttpRequestMessage(HttpMethod.Post, url);

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        message.Headers.Add("x-agent-id", _options.Value.agentId);

        message.Headers.Accept.Clear();

        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

        var request = new
        {
            callbackUrl = _options.Value.CallbackBaseUrl,
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
        var token = await GetAccessTokenChindxAsync(ct);

        var url = _options.Value.DecreaseDirectEndpoint;

        using var message = new HttpRequestMessage(HttpMethod.Post, url);

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        message.Headers.Add("x-agent-id", _options.Value.agentId);

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
        var token = await GetAccessTokenChindxAsync(ct);

        var url = _options.Value.DecreaseVerifyEndpoint;

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

    public async Task<BaseResponse<KarizmahResendDecreasOtpResponseDto>> ResendDecreaseOtpAsync(KarizmahResendDecreasOtpRequestDto req, CancellationToken ct)
    {
        //orderId اینو از سرویس برداشت غیر مستقیم ذخیره کردیم در دیتابیس این تو قسمت اپ میخوتیم و ارسال میکتیم به اینجا
        var client = _http.CreateClient("KarizmahApi");
        var token = await GetAccessTokenChindxAsync(ct);

        var url = $"/api/order/decrease/{req.orderId}/resend-otp";

        using var message = new HttpRequestMessage(HttpMethod.Post, url);

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        message.Headers.Add("x-agent-id", _options.Value.agentId);
        message.Headers.Accept.Clear();
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var request = new KarizmahResendDecreasOtpRequestDto
        {
            nationalCode = req.nationalCode
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        message.Content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await client.SendAsync(message, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"رمز یکبار مصرف نامعتبر است Code={(int)response.StatusCode}, Message={body}");
        }

        var res = JsonSerializer.Deserialize<BaseResponse<KarizmahResendDecreasOtpResponseDto>>(body)
                  ?? throw new InvalidOperationException("رمز یکبار مصرف نامعتبر است");

        return res;
    }


    public async Task<BaseResponse<KarizmahSwapResponseDto>> SwapAsync(KarizmahSwapRequestDto req, CancellationToken ct)
    {
        var traceId = await GenerateTraceIdAsync(ct);
        var token = await GetAccessTokenChindxAsync(ct);

        var url = _options.Value.SwapEnspoint;

        using var message = new HttpRequestMessage(HttpMethod.Post, url);

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        message.Headers.Add("x-agent-id", _options.Value.agentId);

        message.Headers.Accept.Clear();

        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

        var request = new
        {
            nationalCode = req.nationalCode,
            description = req.description,
            sourcePolicyId = req.sourcePolicyId,
            amount = req.amount,
            destinationPolicyId = req.destinationPolicyId,
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

        var bankResponse = JsonSerializer.Deserialize<BaseResponse<KarizmahSwapResponseDto>>(body);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Code={(int)response.StatusCode}, Message={body}");
        }


        if (bankResponse is null)
            throw new InvalidOperationException("پاسخی از بانک دریافت نشد");

        if (!bankResponse.isSuccess)
        {
            return new BaseResponse<KarizmahSwapResponseDto>
            {
                isSuccess = false,
                errorMessages = bankResponse.errorMessages ?? new(),
                data = null!
            };
        }

        return new BaseResponse<KarizmahSwapResponseDto>
        {
            isSuccess = true,
            errorMessages = new List<ErrorMessage>(),
            data = null
        };
    }

    public async Task<BaseResponse<KarizmahOrderTransactionResponseDto>> OrderTransaction(KarizmahOrderTransactionRequestDto req, CancellationToken ct)
    {
        var client = _http.CreateClient("KarizmahApi");

        var token = await GetAccessTokenChindxAsync(ct);

        var query = new Dictionary<string, string>();


        if (req.id.HasValue) query["id"] = req.id.Value.ToString();
        if (!string.IsNullOrEmpty(req.nationalCode)) query["nationalCode"] = req.nationalCode;
        if (!string.IsNullOrEmpty(req.planTypeAliasName)) query["planTypeAliasName"] = req.planTypeAliasName;
        if (!string.IsNullOrEmpty(req.coverageAliasName)) query["coverageAliasName"] = req.coverageAliasName;
        if (req.backofficeOrderId.HasValue) query["backofficeOrderId"] = req.backofficeOrderId.Value.ToString();
        if (req.wealthPolicyId.HasValue) query["wealthPolicyId"] = req.wealthPolicyId.Value.ToString();
        if (req.lifePolicyId.HasValue) query["lifePolicyId"] = req.lifePolicyId.Value.ToString();
        if (req.planTypeId.HasValue) query["planTypeId"] = req.planTypeId.Value.ToString();
        if (req.coverageId.HasValue) query["coverageId"] = req.coverageId.Value.ToString();
        if (!string.IsNullOrEmpty(req.referenceId)) query["referenceId"] = req.referenceId;
        if (req.traceId.HasValue) query["traceId"] = req.traceId.Value.ToString();
        if (!string.IsNullOrEmpty(req.status)) query["status"] = req.status;
        if (!string.IsNullOrEmpty(req.orderType)) query["orderType"] = req.orderType;

        query["page"] = req.page.ToString();

        query["pageSize"] = req.pageSize.ToString();

        var baseUrl = _options.Value.OrderTransactionEnspoint;

        var url = QueryHelpers.AddQueryString(baseUrl, query);

        using var message = new HttpRequestMessage(HttpMethod.Get, url);

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        message.Headers.Add("x-agent-id", _options.Value.agentId);

        message.Headers.Accept.Clear();

        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

        using var response = await client.SendAsync(message, ct);

        var body = await response.Content.ReadAsStringAsync(ct);
        var bankResponse = JsonSerializer.Deserialize<BaseResponse<KarizmahOrderTransactionResponseDto>>(body);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Code={(int)response.StatusCode}, Message={body}");
        }


        if (bankResponse is null)
            throw new InvalidOperationException("پاسخی از بانک دریافت نشد");

        if (!bankResponse.isSuccess)
        {
            return new BaseResponse<KarizmahOrderTransactionResponseDto>
            {
                isSuccess = false,
                errorMessages = bankResponse.errorMessages ?? new(),
                data = null!
            };
        }

        var dto = new KarizmahOrderTransactionResponseDto
        {
            karizmahOrderItems = bankResponse.data.karizmahOrderItems.Select(x => new KarizmahOrderTransactionResponseDto.KarizmahOrderItemDto
            {
                amount = x.amount,
                coverage = x.coverage,
                coverageAliasName = x.coverageAliasName,
                coverageId = x.coverageId,
                createDate = x.createDate,
                id = x.id,
                lifePolicyId = x.lifePolicyId,
                modifyDate = x.modifyDate,
                nationalCode = x.nationalCode,
                orderType = x.orderType,
                planType = x.planType,
                planTypeAliasName = x.planTypeAliasName,
                planTypeId = x.planTypeId,
                referenceId = x.referenceId,
                status = x.status,
                statusTitle = x.statusTitle,
                traceId = x.traceId,
                wealthPolicyId = x.wealthPolicyId
            }).ToList(),
            page = bankResponse.data.page,
            pageSize = bankResponse.data.pageSize,
            totalCount = bankResponse.data.totalCount
        };
        return new BaseResponse<KarizmahOrderTransactionResponseDto>
        {
            isSuccess = true,
            errorMessages = new List<ErrorMessage>(),
            data = dto
        };

    }

    public async Task<BaseResponse<KarizmahOrderResponseDto>> GetOrder(KarizmahOrderRequestDto req, CancellationToken ct)
    {
        var client = _http.CreateClient("KarizmahApi");
        var token = await GetAccessTokenChindxAsync(ct);

        var query = new Dictionary<string, string>();

        if (req.id.HasValue) query["id"] = req.id.Value.ToString();
        if (!string.IsNullOrWhiteSpace(req.planTypeAliasName)) query["planTypeAliasName"] = req.planTypeAliasName;
        if (!string.IsNullOrWhiteSpace(req.coverageAliasName)) query["coverageAliasName"] = req.coverageAliasName;
        if (req.planTypeId.HasValue) query["planTypeId"] = req.planTypeId.Value.ToString();
        if (req.coverageId.HasValue) query["coverageId"] = req.coverageId.Value.ToString();
        if (req.wealthPolicyId.HasValue) query["wealthPolicyId"] = req.wealthPolicyId.Value.ToString();
        if (req.lifePolicyId.HasValue) query["lifePolicyId"] = req.lifePolicyId.Value.ToString();
        if (!string.IsNullOrWhiteSpace(req.nationalCode)) query["nationalCode"] = req.nationalCode;
        if (!string.IsNullOrWhiteSpace(req.phoneNumber)) query["phoneNumber"] = req.phoneNumber;
        if (!string.IsNullOrWhiteSpace(req.customerBankAccountId)) query["customerBankAccountId"] = req.customerBankAccountId;
        if (req.ageFrom.HasValue) query["ageFrom"] = req.ageFrom.Value.ToString();
        if (req.ageTo.HasValue) query["ageTo"] = req.ageTo.Value.ToString();
        if (req.amountFrom.HasValue) query["amountFrom"] = req.amountFrom.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (req.amountTo.HasValue) query["amountTo"] = req.amountTo.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (req.coefficient.HasValue) query["coefficient"] = req.coefficient.Value.ToString();
        if (req.referenceId.HasValue) query["referenceId"] = req.referenceId.Value.ToString();
        if (!string.IsNullOrWhiteSpace(req.status)) query["status"] = req.status;
        if (!string.IsNullOrWhiteSpace(req.orderType)) query["orderType"] = req.orderType;
        if (!string.IsNullOrWhiteSpace(req.description)) query["description"] = req.description;

        if (req.createDateStartDate.HasValue)
            query["createDateStartDate"] = req.createDateStartDate.Value.ToString("O");
        if (req.createDateEndDate.HasValue)
            query["createDateEndDate"] = req.createDateEndDate.Value.ToString("O");
        if (req.modifyDateStartDate.HasValue)
            query["modifyDateStartDate"] = req.modifyDateStartDate.Value.ToString("O");
        if (req.modifyDateEndDate.HasValue)
            query["modifyDateEndDate"] = req.modifyDateEndDate.Value.ToString("O");

        if (!string.IsNullOrWhiteSpace(req.paymentUrl))
            query["PaymentUrl"] = req.paymentUrl;


        query["page"] = req.page.ToString();
        query["pageSize"] = req.pageSize.ToString();

        var baseUrl = _options.Value.OrderEnspoint;
        var url = QueryHelpers.AddQueryString(baseUrl, query);

        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        message.Headers.Add("x-agent-id", _options.Value.agentId);
        message.Headers.Accept.Clear();
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _logger.LogDebug("Calling Karizmah /api/order with URL: {Url}", url);

        using var response = await client.SendAsync(message, ct);
        var json = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("سرویس پاسخ گو نیست" + " status={statusCode}, body={body}", (int)response.StatusCode, json);

            throw new HttpRequestException(
                $"GetOrders failed. Status={(int)response.StatusCode}, body={json}");
        }

        var bankResponse = JsonSerializer.Deserialize<BaseResponse<KarizmahOrderResponseDto>>(json);
        if (bankResponse is null)
        {
            throw new InvalidOperationException("Cannot deserialize GetOrders response.");
        }


        var dto = new KarizmahOrderResponseDto
        {
            karizmahOrderItems = bankResponse.data.karizmahOrderItems.Select(x => new KarizmahOrderResponseDto.KarizmahOrderItemDto
            {
                amount = x.amount,
                coverage = x.coverage,
                coverageAliasName = x.coverageAliasName,
                coverageId = x.coverageId,
                createDate = x.createDate,
                id = x.id,
                lifePolicyId = x.lifePolicyId,
                modifyDate = x.modifyDate,
                nationalCode = x.nationalCode,
                orderType = x.orderType,
                planType = x.planType,
                planTypeAliasName = x.planTypeAliasName,
                planTypeId = x.planTypeId,
                referenceId = x.referenceId,
                status = x.status,
                statusTitle = x.statusTitle,
                traceId = x.traceId,
                wealthPolicyId = x.wealthPolicyId
            }).ToList(),
            page = bankResponse.data.page,
            pageSize = bankResponse.data.pageSize,
            totalCount = bankResponse.data.totalCount
        };
        return new BaseResponse<KarizmahOrderResponseDto>
        {
            isSuccess = true,
            errorMessages = new List<ErrorMessage>(),
            data = dto
        };

    }

    public async Task<BaseResponse<KarizmahOrderRevokableAmountResponseDto>> GetOrderRevokableAmount(KarizmahOrderRevokableAmountRequestDto req, CancellationToken ct)
    {
        var client = _http.CreateClient("KarizmahApi");
        var tokenRes = await GetAccessTokenChindxAsync(ct);

        var endpoint = _options.Value.RevokableAmountEndpoint;
        var url = QueryHelpers.AddQueryString(endpoint, "PolicyId", req.policyId.ToString());

        using var message = new HttpRequestMessage(HttpMethod.Get, url);


        message.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenRes.AccessToken);

        message.Headers.Add("x-agent-id", _options.Value.agentId);

        message.Headers.Accept.Clear();

        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

        _logger.LogDebug("Calling Karizmah revokable-amount API: {Url}", url);

        using var response = await client.SendAsync(message, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Revokable-amount API failed. Status={StatusCode}, Body={Body}",
                (int)response.StatusCode,
                body);

            throw new HttpRequestException(
                $"Revokable-amount API failed. Status={(int)response.StatusCode}, Body={body}");
        }

        var bankResponse =
            JsonSerializer.Deserialize<BaseResponse<KarizmahOrderRevokableAmountResponseDto>>(body);

        if (bankResponse is null)
        {
            throw new InvalidOperationException($"Cannot deserialize revokable-amount response. Body='{body}'");
        }

        var dto = new KarizmahOrderRevokableAmountResponseDto
        {
            revokable = bankResponse.data.revokable,
            value = bankResponse.data.value
        };
        return new BaseResponse<KarizmahOrderRevokableAmountResponseDto>
        {
            isSuccess = true,
            errorMessages = new List<ErrorMessage>(),
            data = dto
        };
    }

    public async Task<BaseResponse<KarizmahPolicyHistoryResponseDto>> GetPolicyHistory(KarizmahPolicyHistoryRequestDto req, CancellationToken ct)
    {
        var client = _http.CreateClient("KarizmahApi");
        var tokenRes = await GetAccessTokenChindxAsync(ct);

        var endpoint = $"/api/policy/{req.policyId}/history";

        var query = new Dictionary<string, string>
        {
            ["fromDate"] = req.fromDate.ToString("yyyy-MM-dd"),
            ["toDate"] = req.toDate.ToString("yyyy-MM-dd")
        };

        var url = QueryHelpers.AddQueryString(endpoint, query);

        using var message = new HttpRequestMessage(HttpMethod.Get, url);

        message.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenRes.AccessToken);

        message.Headers.Add("x-agent-id", _options.Value.agentId);
        message.Headers.Accept.Clear();
        message.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await client.SendAsync(message, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Policy history API failed. Status={(int)response.StatusCode}, Body={body}");
        }

        var bankResponse = JsonSerializer.Deserialize<KarizmahPolicyHistoryResponseDto>(
            body,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        if (bankResponse is null)
            throw new InvalidOperationException("Cannot deserialize policy history response.");

        var dto = new KarizmahPolicyHistoryResponseDto
        {
            Result = bankResponse.data.Result.Select(x => new KarizmahPolicyHistoryResponseDto.KarizmahPolicyHistoryItemDto
            {
                date = x.date,
                endValue = x.endValue,
                pnlValue = x.pnlValue,
                id = x.id,
                insured = x.insured,
                isActive = x.isActive,
                loansValue = x.loansValue,
                penalty = x.penalty,
                pnlRatio = x.pnlRatio,
                revokableAmount = x.revokableAmount,
                solutionType = x.solutionType,
                startValue = x.startValue,
                transactionFee = x.transactionFee,
                value = x.value

            }).ToList()
        };
        return new BaseResponse<KarizmahPolicyHistoryResponseDto>
        {
            isSuccess = true,
            errorMessages = new List<ErrorMessage>(),
            data = dto
        };
    }

    public async Task<ChindxIndexValueResponseDto> GetGoldIndexAsync(ChindxIndexValueRequestDto req, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _http.CreateClient();
            var tokenRes = await GetAccessTokenChindxAsync(cancellationToken);
            if (req is null)
                throw new ArgumentNullException(nameof(req));
            var queryString = BuildQueryString(req);
            var url = _options.Value.BaseUrlchindexApi + $"chindx/v2.0/indexValue{queryString}";
            using var message = new HttpRequestMessage(HttpMethod.Get, url);

            message.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenRes.AccessToken);

            message.Headers.Add("x-agent-id", _options.Value.agentId);
            message.Headers.Accept.Clear();
            message.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            using var response = await client.SendAsync(message, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var bankResponse = JsonSerializer.Deserialize<ChindxIndexValueResponseDto>(
               body,
               new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });


            if (!response.IsSuccessStatusCode)
            {
                var code = (int)response.StatusCode;
                _logger?.LogError(
                    $"در فراخوانی سرویس کاریزما خطا رخ داد Code = {code}, Content = {body}", code, body);

                throw new ExternalServiceException($"در فراخوانی سرویس کاریزما خطا رخ داد.", (int)response.StatusCode, body);
            }

            ChindxIndexValueResponseDto? result;
            try
            {
                result = bankResponse;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex,
                    " در فراخوانی سرویس کاریزما خطا رخ دادContent = {Content}", body);
                throw;
            }

            if (result is null)
            {
                _logger?.LogError("پاسخ سرویس کاریزما خالی یا نامعتبر بود. Content = {Content}", body);
                throw new ExternalServiceException("در فراخوانی سرویس کاریزما خطا رخ داد.", (int)response.StatusCode, body);
            }

            return result;

        }
        catch (Exception ex)
        {

            throw;
        }


    }
    private static string BuildQueryString(ChindxIndexValueRequestDto request)
    {
        var parameters = new List<string>();

        static void AddIfNotNullOrEmpty(string name, string? value, List<string> list)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                list.Add($"{name}={Uri.EscapeDataString(value)}");
            }
        }

        AddIfNotNullOrEmpty("instrumentId", request.InstrumentId, parameters);
        AddIfNotNullOrEmpty("fromDateKey", request.FromDateKey, parameters);
        AddIfNotNullOrEmpty("toDateKey", request.ToDateKey, parameters);

        if (request.FromDate.HasValue)
        {
            var from = request.FromDate.Value.ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture);
            AddIfNotNullOrEmpty("fromDate", from, parameters);
        }

        if (request.ToDate.HasValue)
        {
            var to = request.ToDate.Value.ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture);
            AddIfNotNullOrEmpty("toDate", to, parameters);
        }

        AddIfNotNullOrEmpty("size",
            request.Size.ToString(CultureInfo.InvariantCulture),
            parameters);

        AddIfNotNullOrEmpty("offset",
            request.Offset.ToString(CultureInfo.InvariantCulture),
            parameters);

        return parameters.Count == 0
            ? string.Empty
            : "?" + string.Join("&", parameters);
    }

    public async Task<KarizmaTokenResponse> GetAccessTokenChindxAsync(CancellationToken ct)
    {
        var client = _http.CreateClient();

        var tokenUrl = _options.Value.BaseUrlTokenchindex;

        var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
        {
            Content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope","khanoumi")
        })
        };
        var basicBytes = Encoding.ASCII.GetBytes(
            $"{_options.Value.ConsumerKey}:{_options.Value.ConsumerSecret}");
        var basicToken = Convert.ToBase64String(basicBytes);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Basic", basicToken);

        var resp = await client.SendAsync(request, ct);
        var json = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
        {
            _logger?.LogError(
                "Karizmah chindx token endpoint failed. Status={StatusCode}, Body={Body}",
                (int)resp.StatusCode, json);

            throw new ExternalServiceException(
                $"Karizmah chindx token endpoint failed.", (int)resp.StatusCode, json);
        }

        var tokenResponse = JsonSerializer.Deserialize<KarizmaTokenResponse>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (tokenResponse is null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            throw new InvalidOperationException("Invalid token response from Karizmah chindx.");

        return tokenResponse;
    }

   
}




