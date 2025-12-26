using Common;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.Enum.Investment;
using LoanService.Domain.IRepository.Investment;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanService.Application.UseCase.Investment.Command.PaymentGateway;
//public sealed class StartSadadPaymentHandler(
//    IPaymentRepository repo,
//    //ISadadService gateway,
//    // ISadadCrypto crypto,

//  //  IOptions<SadadOptions> opt,
//    ILogger<StartSadadPaymentHandler> logger
//) : IRequestHandler<StartSadadPaymentCommand, Result<StartSadadPaymentResultDto>>
//{
  //  private readonly IOptions<SadadOptions> _opt = opt;

    //public async Task<Result<StartSadadPaymentResultDto>> Handle(StartSadadPaymentCommand request, CancellationToken ct)
    //{
    //    // Scope log
    //    using var _ = logger.BeginScope(new Dictionary<string, object>
    //    {
    //        ["OrderId"] = request.OrderId,
    //        ["AmountRials"] = request.AmountRials
    //    });


    //    await repo.CreateAsync(new InvestmentPayment

    //    {
    //        OrderId = request.OrderId,
    //        AmountRials = request.AmountRials,
    //        Status = PaymentStatus.Created,
    //        IsFinal = false,

    //        //SadadTokenProtected= null,
    //        //SadadTokenHash= null
    //    }, ct);


    //    //var signPlain = $"{_opt.TerminalId};{request.OrderId};{request.AmountRials}";
    //    //var signData = crypto.EncryptToBase64(signPlain);

    //    var localTehran = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3.5)).DateTime;

    //    // 3) PaymentRequest
    //    var pr = await gateway.PaymentRequestAsync(new SadadPaymentRequestDto
    //    {
    //        //MerchantId = _opt.MerchantId,
    //        //TerminalId = _opt.TerminalId,
    //        //Amount = request.AmountRials,
    //        //OrderId = request.OrderId,
    //        //LocalDateTime = localTehran,
    //        //ReturnUrl = _opt.ReturnUrl,
    //        //SignData = signData,
    //        //AdditionalData = null
    //    }, ct);

    //    //if (pr.ResCode != 0 || string.IsNullOrWhiteSpace(pr.Token))
    //    //{
    //    //    logger.LogWarning("Sadad PaymentRequest failed. ResCode={ResCode}, Desc={Desc}", pr.ResCode, pr.Description);
    //    //    throw new LogicException($"خطا در دریافت توکن از سداد. ({pr.ResCode}) {pr.Description}", AppErrorCodes.LogicError);
    //    //}


    //    //var tokenProtected = protector.Protect(pr.Token);
    //    //var tokenHash = crypto.Sha256(pr.Token);


    //    //var row = await repo.GetByOrderIdAsync(request.OrderId, ct)
    //    //          ?? throw new NotFoundException("Payment not found after create.");

    //    //var ok = await repo.SetTokenAsync(paymentId, tokenProtected, tokenHash, row.RowVersion, ct);
    //    //if (!ok) throw new LogicException("خطا در ثبت توکن پرداخت (Concurrency).", AppErrorCodes.LogicError);

    //    //var redirectUrl = $"{_opt.PurchaseBaseUrl}?Token={Uri.EscapeDataString(pr.Token)}"; 

    //    //logger.LogInformation("Sadad token issued successfully.");

    //    var res = new StartSadadPaymentResultDto();
    //    //: paymentId,
    //    //OrderId: request.OrderId,
    //    //AmountRials: request.AmountRials,
    //    //RedirectUrl: redirectUrl,
    //    //TokenMasked: pr.Token.Length <= 6 ? "***" : $"{pr.Token[..3]}***{pr.Token[^3..]}"
    //    return Result<StartSadadPaymentResultDto>.Success(res);
    //}
//}

