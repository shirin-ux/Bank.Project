using Kavenegar;
using Kavenegar.Exceptions;
using LoanGateway.Auth.Application;
using LoanGateway.Auth.Application.Commons;
using LoanGateway.Auth.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace Sms.Provider
{
    public class SmsSender : ISmsSender
    {
        private readonly KavenegarApi _api;
        private readonly string _sender;
        private readonly ILogger<SmsSender> _logger;
        public SmsSender(IOptions<KavenegarOptions> options, ILogger<SmsSender> logger)
        {
            var otp = options.Value;
            if (string.IsNullOrWhiteSpace(otp.ApiKey))
                throw new ExternalServiceException("Kavenegar", HttpStatusCode.BadGateway, "خطا در ارسال پیامک از طریق سرویس کاوه‌نگار", 502);

            _api = new KavenegarApi(otp.ApiKey);
            _sender = otp.Sender;
            _logger = logger;
        }
        public async Task SendAsync(string mobileNumber, string message, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
                throw new LogicException("شماره موبایل برای ارسال پیامک خالی است.", errorCode: AppErrorCodes.LogicError);

            if (string.IsNullOrWhiteSpace(message))
                throw new LogicException("متن پیامک خالی است.", errorCode: AppErrorCodes.NotFound);
            try
            {
                await Task.Run(() =>
                {
                    _api.Send(_sender, mobileNumber, message);

                }, ct);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "خطای ApiException در ارسال SMS با کاوه‌نگار");

                throw new ExternalServiceException(
                    externalSystem: "Kavenegar",
                    externalStatusCode: HttpStatusCode.BadGateway,
                    message: "خطا در ارسال پیامک از طریق سرویس کاوه‌نگار.",
                  errorCode: AppErrorCodes.ExternalServiceError,
                    payload: new
                    {
                        mobileNumber,
                        ex.Message
                    },
                    innerException: ex);
            }
            catch (HttpException ex)
            {
                _logger.LogError(ex, "خطای HttpException در ارتباط با سرویس کاوه‌نگار");

                throw new ExternalServiceException(
                    externalSystem: "Kavenegar",
                    externalStatusCode: HttpStatusCode.ServiceUnavailable,
                    message: "امکان برقراری ارتباط با سرویس پیامک کاوه‌نگار وجود ندارد.",
                    errorCode: AppErrorCodes.ExternalServiceTimeout,
                    payload: new
                    {
                        mobileNumber,
                        ex.Message
                    },
                    innerException: ex);
            }
            catch (TaskCanceledException ex)
            {

                _logger.LogWarning(ex, "ارسال SMS به شماره {Mobile} به علت لغو/Timeout متوقف شد.", mobileNumber);
                throw;
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "خطای ناشناخته در SmsSender هنگام ارسال پیامک به {Mobile}", mobileNumber);

                throw new ExternalServiceException(
                    externalSystem: "Kavenegar",
                    externalStatusCode: HttpStatusCode.BadGateway,
                    message: "خطای نامشخص در ارسال پیامک رخ داد.",
                    errorCode: AppErrorCodes.ExternalServiceError,
                    payload: new { mobileNumber },
                    innerException: ex);
            }
        }
    }

}


