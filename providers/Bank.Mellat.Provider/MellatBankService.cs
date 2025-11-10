using Bank.Mellat.Provider.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
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
using static Bank.Mellat.Provider.Dtos.MellatTransferRegisterReq;


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

            var response = await client.PostAsync(_options.Value.BaseUrlApi + "/api/fs-contract-management/hub/customer-inquiry", content, ct);

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
    public async Task<(bool IsSuccess, MellatInquiryResultRes? Result, string? Error)> GetInquiryResultAsync(string requestId, CancellationToken ct)
    {
        var client = await CreateApiClientAsync(ct);

        using var msg = new HttpRequestMessage(HttpMethod.Get, _options.Value.BaseUrlApi + $"/api/fs-contract-management/hub/customer-inquiry/{requestId}");

        using var res = await client.SendAsync(msg, ct);
        string? errorBody = null;
        if (!res.IsSuccessStatusCode)
        {
            errorBody = await res.Content.ReadAsStringAsync(ct);
            return (IsSuccess: false, Result: null, Error: $"Bank API responded with {(int)res.StatusCode} ({res.ReasonPhrase}). Response Body: {errorBody}");
        }
        var content = await res.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Bank API returned an empty response body.");
        try
        {
            var result = JsonSerializer.Deserialize<MellatInquiryResultRes>(content, _json);

            return result != null ? (true, result, null) : (false, null, $"Bank API returned null response. Response Body: {content}");
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
            // cbTrackingCode = req.cbTrackingCode
        };

        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(_options.Value.BaseUrlApi + "/api/fs-contract-management/hub/mellat-contract-file", content, ct);

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

        if (!res.IsSuccessStatusCode)
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

        if (!res.IsSuccessStatusCode)
        {
            var errorBody = await res.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Bank API responded with {(int)res.StatusCode} ({res.ReasonPhrase}). " + $"Response Body: {errorBody}");
        }

        var content = await res.Content.ReadAsStringAsync(ct);

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

        try
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
            }); ;

            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = _options.Value.BaseUrlApi + "/api/fs-contract-management/hub/customer-credit-balance";
            using var res = await client.PostAsync(url, content, ct);

            var responseText = await res.Content.ReadAsStringAsync(ct);


            var responseBank = JsonSerializer.Deserialize<MellatCustomerCreditBalanceRes>(responseText);

            return responseBank;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Mellat Inquiry Register");
            throw;
        }

    }
    public async Task<MellatOtpRes> RequestOtpAsync(MellatOtpReq req, CancellationToken ct)
    {

        try
        {
            var client = _http.CreateClient("MellatApi");
            var token = await GetAccessTokenAsync(ct);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var body = new
            {
                contractNumber = req.contractNumber,
                nationalCode = req.nationalCode,
                payAmount = Math.Round(req.payAmount, 0),
                serviceType = req.serviceType,
                accountNumber = req.accountNumber
            };

            var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }); ;

            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = _options.Value.BaseUrlApi + "/api/fs-contract-management/hub/otp-request";
            using var res = await client.PostAsync(url, content, ct);

            var responseText = await res.Content.ReadAsStringAsync(ct);


            var responseBank = JsonSerializer.Deserialize<MellatOtpRes>(responseText);

            return responseBank;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Mellat Inquiry Register");
            throw;
        }

    }
    public async Task<MellatDepositRes> RequestDepositAsync(MellatDepositReq req, CancellationToken ct)
    {

        try
        {
            var client = _http.CreateClient("MellatApi");
            var token = await GetAccessTokenAsync(ct);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var body = new
            {
                contractNumber = req.contractNumber,
                buyerNationalCode = req.buyerNationalCode,
                sellerNationalCode = req.sellerNationalCode,
                otpCode = req.otpCode,
                depositType = req.depositType,
                payAmount = req.payAmount,
                sellerAccountNo = req.sellerAccountNo,
                transactionDesc = req.transactionDesc
            };

            var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }); ;

            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = _options.Value.BaseUrlApi + "/api/fs-contract-management/hub/deposit-request";
            using var res = await client.PostAsync(url, content, ct);

            var responseText = await res.Content.ReadAsStringAsync(ct);


            var responseBank = JsonSerializer.Deserialize<MellatDepositRes>(responseText);

            return responseBank;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Mellat Inquiry Register");
            throw;
        }

    }

    public async Task<MellatRepaymentRes> RepaymentRequestAsync(MellatRepaymentReq req, CancellationToken ct)
    {

        try
        {
            var client = _http.CreateClient("MellatApi");
            var token = await GetAccessTokenAsync(ct);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var body = new
            {
                accountNo = req.accountNo,
                contractNo = req.contractNo,
                nationalCode = req.nationalCode,
                otpCode = req.otpCode,
                repaymentAmount = req.repaymentAmount
            };

            var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }); ;

            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = _options.Value.BaseUrlApi + "/api/fs-contract-management/hub/repayment-request";
            using var res = await client.PostAsync(url, content, ct);

            var responseText = await res.Content.ReadAsStringAsync(ct);


            var responseBank = JsonSerializer.Deserialize<MellatRepaymentRes>(responseText);

            return responseBank;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Mellat Inquiry Register");
            throw;
        }

    }

    public async Task<MellatCustomerBillingRes> GetCustomerBillingAsync(MellatCustomerBillingReq req, CancellationToken ct)
    {
        try
        {
            var client = _http.CreateClient("MellatApi");
            var token = await GetAccessTokenAsync(ct);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var body = new
            {
                contractNumber = req.contractNumber,
                billingNumber = req.billingNumber,
                nationalCode = req.nationalCode
            };

            var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }); ;

            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = _options.Value.BaseUrlApi + "/api/fs-contract-management/hub/customer-billing";
            using var res = await client.PostAsync(url, content, ct);

            var responseText = await res.Content.ReadAsStringAsync(ct);


            var responseBank = JsonSerializer.Deserialize<MellatCustomerBillingRes>(responseText);

            return responseBank;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Mellat Inquiry Register");
            throw;
        }

    }

    public async Task<MellatCustomerPurchaseDetailsRes> CustomerPurchaseDetailsAsync(MellatCustomerPurchaseDetailsReq req, CancellationToken ct)
    {
        try
        {
            var client = _http.CreateClient("MellatApi");
            var token = await GetAccessTokenAsync(ct);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var body = new
            {
                contractNumber = req.contractNumber,
                toDate = req.toDate,
                fromDate = req.fromDate,
                nationalCode = req.nationalCode
            };

            var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }); ;

            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = _options.Value.BaseUrlApi + "/api/fs-contract-management/hub/customer-purchase-details";
            using var res = await client.PostAsync(url, content, ct);

            var responseText = await res.Content.ReadAsStringAsync(ct);


            var responseBank = JsonSerializer.Deserialize<MellatCustomerPurchaseDetailsRes>(responseText);

            return responseBank;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Mellat Inquiry Register");
            throw;
        }
    }

    public async Task<MellatTransferRegisterRes> RegisterTransferAsync(MellatTransferRegisterReq req, CancellationToken ct)
    {

        try
        {
            var client = _http.CreateClient("MellatApi");
            var token = await GetAccessTokenAsync(ct);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var body = new
            {
                approvalCode = req.approvalCode,
                transferDate = req.transferDate,
                payAmount = req.payAmount,
                destIban = req.destIban,
                destNationalId = req.destNationalId,
                destName = req.destName,
                description = req.description,
                details = req.details.Select(x => new contractDetails
                {
                    amount = x.amount,
                    referenceNo = x.referenceNo
                }).ToList()
            };

            var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }); ;

            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = _options.Value.BaseUrlApi + "/api/fs-contract-management/hub/transfer-register";
            using var res = await client.PostAsync(url, content, ct);

            var responseText = await res.Content.ReadAsStringAsync(ct);


            var responseBank = JsonSerializer.Deserialize<MellatTransferRegisterRes>(responseText);

            return responseBank;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Mellat Inquiry Register");
            throw;
        }
    }

    public async Task<(bool IsSuccess, MellatTransferInquiryRes? Result, string? Error)> GetTransferInquiryAsync(MellatTransferInquiryReq req, CancellationToken ct)
    {

        var client = await CreateApiClientAsync(ct);

        using var msg = new HttpRequestMessage(HttpMethod.Get, _options.Value.BaseUrlApi + $"/api/fs-contract-management/hub/return-transfer-report");

        using var res = await client.SendAsync(msg, ct);
        string? errorBody = null;
        if (!res.IsSuccessStatusCode)
        {
            errorBody = await res.Content.ReadAsStringAsync(ct);
            return (IsSuccess: false, Result: null, Error: $"Bank API responded with {(int)res.StatusCode} ({res.ReasonPhrase}). Response Body: {errorBody}");
        }
        var content = await res.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Bank API returned an empty response body.");
        try
        {
            var result = JsonSerializer.Deserialize<MellatTransferInquiryRes>(content, _json);

            return result != null ? (true, result, null) : (false, null, $"Bank API returned null response. Response Body: {content}");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse JSON response: {ex.Message}\nResponse: {content}", ex);
        }
    }

    public async Task<MellatSubmitPayRequestRes> SubmitPayRequestAsync(MellatSubmitPayRequestReq req, CancellationToken ct)
    {
        var client = _http.CreateClient("MellatApi");
        var token = await GetAccessTokenAsync(ct);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);



        var body = new
        {
            contractNumber = req.contractNumber,
            requestAmount = req.requestAmount,
            contractFile = req.contractFile
        };

        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await client.PostAsync(_options.Value.BaseUrlApi + "/api/fs-contract-management/hub/contract-pay-request", content, ct);



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

        MellatSubmitPayRequestRes? responseBank;
        try
        {
            responseBank = JsonSerializer.Deserialize<MellatSubmitPayRequestRes>(responseText);
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

    public async Task<(bool IsSuccess, MellatReturnTransferReportRes? Result, string? Error)>  GetReturnTransferReportAsync(MellatReturnTransferReportReq req, CancellationToken ct)
    {



        var client = await CreateApiClientAsync(ct);

        using var msg = new HttpRequestMessage(HttpMethod.Get, _options.Value.BaseUrlApi + $"/api/fs-contract-management/hub/return-transfer-report");

        using var res = await client.SendAsync(msg, ct);
        string? errorBody = null;
        if (!res.IsSuccessStatusCode)
        {
            errorBody = await res.Content.ReadAsStringAsync(ct);
            return (IsSuccess: false, Result: null, Error: $"Bank API responded with {(int)res.StatusCode} ({res.ReasonPhrase}). Response Body: {errorBody}");
        }
        var content = await res.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Bank API returned an empty response body.");
        try
        {
            var result = JsonSerializer.Deserialize<MellatReturnTransferReportRes>(content, _json);

            return result != null ? (true, result, null) : (false, null, $"Bank API returned null response. Response Body: {content}");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse JSON response: {ex.Message}\nResponse: {content}", ex);
        }

    }

}