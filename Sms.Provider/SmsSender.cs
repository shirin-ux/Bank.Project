using Kavenegar;
using LoanGateway.Auth.Application;
using LoanGateway.Auth.Application.Common;
using MediatR;
using Microsoft.Extensions.Options;

namespace Sms.Provider
{
    public class SmsSender : ISmsSender
    {
        private readonly KavenegarApi _api;
        private readonly string _sender;
        public SmsSender(IOptions<KavenegarOptions> options)
        {
            var opt = options.Value;

            if (string.IsNullOrWhiteSpace(opt.ApiKey))
                throw new InvalidOperationException("Kavenegar ApiKey is not configured.");

            _api = new KavenegarApi(opt.ApiKey);
            _sender = opt.Sender;
        }
        public async Task SendAsync(string mobileNumber, string message, CancellationToken ct)
        {
            await Task.Run(() =>
            {
                _api.Send(_sender, mobileNumber, "123456");

            }, ct);
        }
    }
}
