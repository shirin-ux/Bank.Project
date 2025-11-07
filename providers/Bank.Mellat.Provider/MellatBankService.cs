using Bank.Mellat.Provider.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Unicode;


namespace Bank.Mellat.Provider;

public sealed class MellatBankService(IHttpClientFactory http, IOptions<MellatApiOptions> options, ILogger<MellatBankService> logger) : IMellatBankService
{

    private readonly IHttpClientFactory _http = http;
    private readonly IOptions<MellatApiOptions> _options = options;
    private string? _cachedToken;
    private DateTimeOffset _tokenExpiresAt;
    private ILogger<MellatBankService> _logger = logger;
    private string? _jsessionId;
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = null,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

    };

    public async Task<MellatInquiryRegisterRes> RegisterInquiryAsync(MellatInquiryRegisterReq req, CancellationToken ct)
    {
        try
        {
            var client = _http.CreateClient("MellatApi");
            var token = await GetAccessTokenAsync(ct);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var body = new
            {
                nationalCode = req.nationalCode,
                birthDate = req.birthDate,
                requestAmount = req.requestAmount,
                mobileNo = req.mobileNo
            };

            var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(_options.Value.BaseUrlApi+"/api/fs-contract-management/hub/customer-inquiry", content, ct);

            var responseText = await response.Content.ReadAsStringAsync(ct);


            var responseBank = JsonSerializer.Deserialize<MellatInquiryRegisterRes>(responseText);
            if (responseBank.messageCode == 11112)
            {
                _logger.LogError("Mellat API Error: {Status} - {Response}", response.StatusCode, responseText);
                throw new InvalidOperationException($"Error {response.StatusCode}: {responseBank.messageCode} -{responseBank.message}");

            }
            return responseBank;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Mellat Inquiry Register");
            throw;
        }

    }
    public async Task<string> GetAccessTokenAsync(CancellationToken ct)
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
        var tokenResponse = JsonSerializer.Deserialize<MellatTokenRes>(json);
        return tokenResponse.AccessToken;
    }
    private async Task<HttpClient> CreateApiClientAsync(CancellationToken ct)
    {
        var client = _http.CreateClient();
        var token = await GetAccessTokenAsync(ct);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
    public async Task<MellatInquiryResultRes> GetInquiryResultAsync(string requestId, CancellationToken ct)
    {
        var client = await CreateApiClientAsync(ct);

        using var msg = new HttpRequestMessage(HttpMethod.Get, _options.Value.BaseUrlApi + $"/api/fs-contract-management/hub/customer-inquiry/{requestId}");

        using var res = await client.SendAsync(msg, ct);

        if (!res.IsSuccessStatusCode)
        {
            var errorBody = await res.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Bank API responded with {(int)res.StatusCode} ({res.ReasonPhrase}). " +
                $"Response Body: {errorBody}"
            );
        }
        var content = await res.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Bank API returned an empty response body.");
        try
        {
            var result = JsonSerializer.Deserialize<MellatInquiryResultRes>(content, _json);

            return result ?? throw new InvalidOperationException("Response body could not be parsed into MellatInquiryResultRes.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse JSON response: {ex.Message}\nResponse: {content}", ex);
        }
    }
    public async Task<MellatFileUploadRes> UploadContractFileAsync(MellatFileUploadReq req, CancellationToken ct)
    {
        var client = _http.CreateClient("MellatApi");
        var token = await GetAccessTokenAsync(ct);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var body = new
        {
            nationalCode = req.nationalCode,
            birthDate = req.birthDate,
            mobileNumber = req.mobileNumber,
            approvalCode = req.approvalCode,
            postalCode = req.postalCode,
            phoneNumber = req.phoneNumber,
            loanAmount = req.loanAmount,
            installmentCount = req.installmentCount,
            address = req.address,
            cbTrackingCode = req.cbTrackingCode
        };

        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await client.PostAsync(_options.Value.BaseUrlApi + "/api/fs-contract-management/hub/mellat-contract-file", content, ct);
        var responseText = await response.Content.ReadAsStringAsync(ct);


        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Bank API responded with {(int)response.StatusCode} ({response.ReasonPhrase}). " +
                $"Response Body: {errorBody}"
            );
        }
     

        if (string.IsNullOrWhiteSpace(responseText))
        {
            _logger.LogError("Mellat API empty response.");
            throw new InvalidOperationException("Mellat API returned empty body.");
        }

        MellatFileUploadRes? responseBank;
        try
        {
            responseBank = JsonSerializer.Deserialize<MellatFileUploadRes>(responseText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot deserialize Mellat API response: {Body}", responseText);
            throw new InvalidOperationException("Cannot deserialize Mellat API response.");
        }

        if (responseBank is null)
        {
            _logger.LogError("Mellat API response deserialized to null. Body: {Body}", responseText);
            throw new InvalidOperationException("Mellat API response is null.");
        }
        return responseBank;
    }
    public async Task<MellatFileUploadRes> UploadCollateralFileAsync(MellatContractWithCollateralReq req, CancellationToken ct)
    {
        var client = _http.CreateClient("MellatApi");
        var token = await GetAccessTokenAsync(ct);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


        var body = new
        {
            nationalCode = req.nationalCode,
            birthDate = req.birthDate,
            mobileNumber = req.mobileNumber,
            approvalCode = req.approvalCode,
            postalCode = req.postalCode,
            phoneNumber = req.phoneNumber,
            loanAmount = req.loanAmount,
            collateralType = req.collateralType,
            collateralNo = req.collateralNo,
            collateralDate = req.collateralDate,
            collateralAmount = req.collateralAmount,
            guarantorNC = req.guarantorNC,
            installmentCount = req.installmentCount,
            address = req.address,
            cbTrackingCode = req.cbTrackingCode,
            collateralIssuer = req.collateralIssuer,
            chequeSerial = req.chequeSerial
        };

        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await client.PostAsync(_options.Value.BaseUrlApi + "/api/fs-contract-management/hub/mellat-contract-file-collateral", content, ct);
        var responseText = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Bank API responded with {(int)response.StatusCode} ({response.ReasonPhrase}). " +
                $"Response Body: {errorBody}"
            );
        }


        if (string.IsNullOrWhiteSpace(responseText))
        {
            _logger.LogError("Mellat API empty response.");
            throw new InvalidOperationException("Mellat API returned empty body.");
        }

        MellatFileUploadRes? responseBank;
        try
        {
            responseBank = JsonSerializer.Deserialize<MellatFileUploadRes>(responseText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot deserialize Mellat API response: {Body}", responseText);
            throw new InvalidOperationException("Cannot deserialize Mellat API response.");
        }

        if (responseBank is null)
        {
            _logger.LogError("Mellat API response deserialized to null. Body: {Body}", responseText);
            throw new InvalidOperationException("Mellat API response is null.");
        }
        return responseBank;
    }
    public async Task<MellatPayResponseRes> GetPayResponseAsync(MellatPayReq req, CancellationToken ct)
    {
        var client = _http.CreateClient("MellatApi");
        var token = await GetAccessTokenAsync(ct);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var url = _options.Value.BaseUrlApi + $"/api/fs-contract-management/contract/pay-response/{req.PayRequestId}";

        using var msg = new HttpRequestMessage(HttpMethod.Get, url);

        using var res = await client.SendAsync(msg, ct);

        if(!res.IsSuccessStatusCode)
        {
            var errorBody = await res.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Bank API responded with {(int)res.StatusCode} ({res.ReasonPhrase}). " + $"Response Body: {errorBody}");
        }

        var content = await res.Content.ReadAsStringAsync(ct);

        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Bank API returned an empty response body.");

        try
        {
            var result = JsonSerializer.Deserialize<MellatPayResponseRes>(content, _json);

            return result ?? throw new InvalidOperationException("Response body could not be parsed into MellatPayResponseRes.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse JSON response: {ex.Message}\nResponse: {content}", ex);
        }
    }
    public async Task<MellatInstallmentsRes> GetInstallmentsAsync(MellatInstallmentsReq req, CancellationToken ct)
    {
        var client = _http.CreateClient("MellatApi");
        var token = await GetAccessTokenAsync(ct);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);


        var url = _options.Value.BaseUrlApi + $"/fs-contract-management/hub/installment/installments/{req.NationalCode}/{req.ContractNumber}";

        var msg = new HttpRequestMessage(HttpMethod.Get, url);

        using var res = await client.SendAsync(msg, ct);

        if(!res.IsSuccessStatusCode)
        {
            var errorBody = await res.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Bank API responded with {(int)res.StatusCode} ({res.ReasonPhrase}). " + $"Response Body: {errorBody}");
        }

        var content =await res.Content.ReadAsStringAsync(ct);

        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Bank API returned an empty response body.");
        try
        {
            var result = JsonSerializer.Deserialize<MellatInstallmentsRes>(content, _json);

            return result ?? throw new InvalidOperationException("Response body could not be parsed into MellatInstallmentsRes.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse JSON response: {ex.Message}\nResponse: {content}", ex);
        }
    }
    public async Task<MellatCustomerCreditBalanceRes> GetCustomerCreditBalanceAsync(MellatCustomerCreditBalanceReq request, CancellationToken ct)
    {
        var client = _http.CreateClient("MellatApi");
        var token = await GetAccessTokenAsync(ct);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var body = new
        {
            nationalCode = request.nationalCode,
            contractNumber = request.contractNumber

        };

        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var url = _options.Value.BaseUrlApi + "/fs-contract-management/hub/customer-credit-balance";

        using var content = new StringContent(json, Encoding.UTF8, "application/json");


        using var res = await client.PostAsync(url, content,ct);

        res.EnsureSuccessStatusCode();

        var response = await res.Content.ReadFromJsonAsync<MellatCustomerCreditBalanceRes>(_json, ct)
                       ?? throw new InvalidOperationException("Empty response body");

        return response;
    }
    public async Task<MellatOtpRes> RequestOtpAsync(MellatOtpReq req, CancellationToken ct)
    {
        var client = await CreateApiClientAsync(ct);
        using var msg = new HttpRequestMessage(HttpMethod.Post, _options.Value.BaseUrlApi + "/hub/otp-request")
        { Content = JsonContent.Create(req, options: _json) };

        //msg.Headers.TryAddWithoutValidation("CorrelationId", req.CorrelationId);

        using var res = await client.SendAsync(msg, ct);
        var json = await res.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<MellatOtpRes>(json);

        return (await res.Content.ReadFromJsonAsync<MellatOtpRes>(_json, ct))
               ?? throw new InvalidOperationException("Empty body");
    }
    public async Task<MellatDepositRes> RequestDepositAsync(MellatDepositReq req, CancellationToken ct)
    {
        var client = await CreateApiClientAsync(ct);
        //msg.Headers.TryAddWithoutValidation("CorrelationId", req.CorrelationId);
        var otpCodeResponse = await RequestOtpAsync(new MellatOtpReq
        {
            AccountNumber = req.SellerAccountNo.ToString(),
            NationalCode = req.SellerNationalCode,
            ContractNumber = req.ContractNumber,
            PayAmount = req.PayAmount
        }, ct);

        req.OtpCode = otpCodeResponse.OtpCode;
        using var msg = new HttpRequestMessage(HttpMethod.Post, _options.Value.BaseUrlApi + "/hub/deposit-request") { Content = JsonContent.Create(req, options: _json) };
        using var res = await client.SendAsync(msg, ct);
        var json = await res.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<MellatDepositRes>(json);

        return (await res.Content.ReadFromJsonAsync<MellatDepositRes>(_json, ct))
               ?? throw new InvalidOperationException("Empty body");
    }

    public async Task<MellatRepaymentRes> RequestRepaymentAsync(MellatRepaymentReq req, CancellationToken ct)
    {
        var client = await CreateApiClientAsync(ct);
        //msg.Headers.TryAddWithoutValidation("CorrelationId", req.CorrelationId);
        var otpCodeResponse = await RequestOtpAsync(new MellatOtpReq
        {
            AccountNumber = req.AccountNo.ToString(),
            NationalCode = req.NationalCode,
            ContractNumber = req.ContractNo,
            PayAmount = req.RepaymentAmount
        }, ct);

        req.OtpCode = otpCodeResponse.OtpCode;
        using var msg = new HttpRequestMessage(HttpMethod.Post, _options.Value.BaseUrlApi + "/hub/repayment-request") { Content = JsonContent.Create(req, options: _json) };
        using var res = await client.SendAsync(msg, ct);
        var json = await res.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<MellatRepaymentRes>(json);

        return (await res.Content.ReadFromJsonAsync<MellatRepaymentRes>(_json, ct))
               ?? throw new InvalidOperationException("Empty body");
    }

    public async Task<MellatCustomerBillingRes> GetCustomerBillingAsync(MellatCustomerBillingReq req, CancellationToken ct)
    {
        var client = await CreateApiClientAsync(ct);
        using var msg = new HttpRequestMessage(HttpMethod.Post, _options.Value.BaseUrlApi + "/hub/customer-billing") { Content = JsonContent.Create(req, options: _json) };
        using var res = await client.SendAsync(msg, ct);
        var json = await res.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<MellatCustomerBillingRes>(json);

        return (await res.Content.ReadFromJsonAsync<MellatCustomerBillingRes>(_json, ct))
               ?? throw new InvalidOperationException("Empty body");
    }

    public async Task<MellatCustomerPurchaseDetailsRes> CustomerPurchaseDetailsAsync(MellatCustomerPurchaseDetailsReq req, CancellationToken ct)
    {
        var client = await CreateApiClientAsync(ct);
        using var msg = new HttpRequestMessage(HttpMethod.Post, _options.Value.BaseUrlApi + "/hub/customer-purchase-details") { Content = JsonContent.Create(req, options: _json) };
        using var res = await client.SendAsync(msg, ct);
        var json = await res.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<MellatCustomerPurchaseDetailsRes>(json);

        return (await res.Content.ReadFromJsonAsync<MellatCustomerPurchaseDetailsRes>(_json, ct))
               ?? throw new InvalidOperationException("Empty body");
    }

    public async Task<MellatTransferRegisterRes> RegisterTransferAsync(MellatTransferRegisterReq req, CancellationToken ct)
    {
        var client = await CreateApiClientAsync(ct);
        using var msg = new HttpRequestMessage(HttpMethod.Post, _options.Value.BaseUrlApi + "/transfer/transfer-register") { Content = JsonContent.Create(req, options: _json) };
        using var res = await client.SendAsync(msg, ct);
        var json = await res.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<MellatTransferRegisterRes>(json);

        return (await res.Content.ReadFromJsonAsync<MellatTransferRegisterRes>(_json, ct))
               ?? throw new InvalidOperationException("Empty body");
    }

    public async Task<MellatTransferInquiryRes> GetTransferInquiryAsync(MellatTransferInquiryReq req, CancellationToken ct)
    {
        var client = await CreateApiClientAsync(ct);
        var url = _options.Value.BaseUrlApi + $"/transfer/transfer-inquiry?registerCode={req.RegisterCode}";
        using var res = await client.GetAsync(url, ct);
        res.EnsureSuccessStatusCode();
        var response = await res.Content.ReadFromJsonAsync<MellatTransferInquiryRes>(_json, ct)
                       ?? throw new InvalidOperationException("Empty response body");
        return response;
    }

    public async Task<MellatSubmitPayRequestRes> SubmitPayRequestAsync(MellatSubmitPayRequestReq req, CancellationToken ct)
    {
        var client = await CreateApiClientAsync(ct);
        using var msg = new HttpRequestMessage(HttpMethod.Post, _options.Value.BaseUrlApi + "/hub/contract-pay-request") { Content = JsonContent.Create(req, options: _json) };
        using var res = await client.SendAsync(msg, ct);
        var json = await res.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<MellatSubmitPayRequestRes>(json);

        return (await res.Content.ReadFromJsonAsync<MellatSubmitPayRequestRes>(_json, ct))
               ?? throw new InvalidOperationException("Empty body");
    }

    public async Task<MellatReturnTransferReportRes> GetReturnTransferReportAsync(MellatReturnTransferReportReq req, CancellationToken ct)
    {

        var client = await CreateApiClientAsync(ct);
        using var msg = new HttpRequestMessage(HttpMethod.Post, _options.Value.BaseUrlApi + "/hub/return-transfer-report")
        { Content = JsonContent.Create(req, options: _json) };
        using var res = await client.SendAsync(msg, ct);
        var json = await res.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<MellatReturnTransferReportRes>(json);

        return (await res.Content.ReadFromJsonAsync<MellatReturnTransferReportRes>(_json, ct))
               ?? throw new InvalidOperationException("Empty body");
    }

}