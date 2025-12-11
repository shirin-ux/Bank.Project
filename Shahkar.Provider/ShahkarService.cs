using LoanGateway.Auth.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shahkar.Provider.Dto;
using System.Globalization;
using System.Net;
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
                nationalId = nationalId,
                mobileNumber = mobileNumber,
                requestContext = new
                {
                    apiInfo = new
                    {
                        businessId = _options.Value.BusinessId,
                        businessToken = _options.Value.BusinessToken
                    }
                }

            };
            var url = _options.Value.BaseUrl + "/api/inquiry/mobile/owner/v2";

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
                throw new ExternalServiceException(
                    externalSystem: "UID-Shahkar",
                    externalStatusCode: response.StatusCode,
                    message: "خطا در تماس با وب‌سرویس شاهکار.",
                    errorCode: AppErrorCodes.ExternalServiceError);
            }

            var result = JsonSerializer.Deserialize<ShahkarMatchResponseDto>(responseBody, _json);

            if (result is null)
            {
                throw new ExternalServiceException(
                    "UID-Shahkar",
                    HttpStatusCode.BadGateway,
                    "پاسخ نامعتبر از وب‌سرویس شاهکار.",
                    AppErrorCodes.ExternalServiceError);
            }

            return new ShahkarMatchResponseDto
            {
                isMatched = result.isMatched,
                responseContext = new ResponseContextDto
                {
                    correlationId = result.responseContext.correlationId,
                    custom = result.responseContext.custom,
                    status = result.responseContext.status,
                    navigationURI = result.responseContext.navigationURI,
                    nextStepToken = result.responseContext.nextStepToken,
                    requestId = result.responseContext.requestId,
                    userSessionId = result.responseContext.userSessionId

                }
            };
        }

        public async Task<ShahkarGetPersonInfoResponseDto> GetPersonalInfoAsync(string nationalId, string birthDate, CancellationToken cancellationToken = default)
        {
            var client = _http.CreateClient();
            var req = new
            {
                nationalId = nationalId,
                birthDate = birthDate,
                requestContext = new
                {
                    apiInfo = new
                    {
                        businessId = _options.Value.BusinessId,
                        businessToken = _options.Value.BusinessToken
                    }
                }

            };
            var url = _options.Value.BaseUrl + "/api/inquiry/person/v2";

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
                throw new ExternalServiceException(
                    "UID-PersonalInfo",
                    response.StatusCode,
                    "خطا در تماس با سرویس personal-info.",
                    AppErrorCodes.ExternalServiceError);
            }

            var result = JsonSerializer.Deserialize<ShahkarGetPersonInfoResponseDto>(
                responseBody, _json);

            if (result is null)
            {
                throw new ExternalServiceException(
                    "UID-PersonalInfo",
                    HttpStatusCode.BadGateway,
                    "پاسخ نامعتبر از سرویس personal-info.",
                    AppErrorCodes.ExternalServiceError);
            }
            var status = result.responseContext.status;
            if (status.code != 0)
            {
                throw new ExternalServiceException(
                    "UID-PersonalInfo",
                    HttpStatusCode.BadGateway,
                    $"استعلام اطلاعات هویتی ناموفق بود: {status.message}",
                    AppErrorCodes.ExternalServiceError,
                    payload: status);
            }
        

            return new ShahkarGetPersonInfoResponseDto
            {
                basicInformation = new ShahkarGetPersonInfoResponseDto.BasicInformationDto
                {
                    fatherName = result.basicInformation.fatherName,
                    firstName = result.basicInformation.firstName,
                    gender = result.basicInformation.gender,
                    lastName = result.basicInformation.lastName,

                },
                identificationInformation=new ShahkarGetPersonInfoResponseDto.IdentificationInformationDto
                {
                    birthDate= result.identificationInformation.birthDate,
                    nationalId=result.identificationInformation.nationalId,
                    shenasnamehNumber=result.identificationInformation.shenasnamehNumber,
                    shenasnameSeri=result.identificationInformation.shenasnameSerial,
                    shenasnameSerial=result.identificationInformation.shenasnameSerial,
                    
                },
                officeInformation=new ShahkarGetPersonInfoResponseDto.OfficeInformationDto
                {
                    officeCode=result.officeInformation.officeCode,
                    officeName=result.officeInformation.officeName
                },
                registrationStatus=new ShahkarGetPersonInfoResponseDto.RegistrationStatusDto
                {
                    deathStatus=result.registrationStatus.deathStatus,
                    
                },
                responseContext=new ShahkarGetPersonInfoResponseDto.ResponseContextDto
                {
                    correlationId = result.responseContext.correlationId,
                    custom = result.responseContext.custom,
                    status = result.responseContext.status,
                    navigationURI = result.responseContext.navigationURI,
                    nextStepToken = result.responseContext.nextStepToken,
                    requestId = result.responseContext.requestId,
                    userSessionId = result.responseContext.userSessionId
                }
            };
        }
    }
}
