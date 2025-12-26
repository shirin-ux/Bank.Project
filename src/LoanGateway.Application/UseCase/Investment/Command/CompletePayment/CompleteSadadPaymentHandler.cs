namespace LoanService.Application.UseCase.Investment.Command.CompletePayment
{
    //public sealed class CompleteSadadPaymentHandler(
    //    IPaymentRepository repo,
    //    ISadadService gateway,
    //    //ISadadCrypto crypto,
    //    //IDataProtector protector,
    //    ILogger<CompleteSadadPaymentHandler> logger
    //) : IRequestHandler<CompleteSadadPaymentCommand, Result<CompleteSadadPaymentResultDto>>
    //{
    //    public async Task<Result<CompleteSadadPaymentResultDto>> Handle(CompleteSadadPaymentCommand request, CancellationToken ct)
    //    {
    //        var cb = request.Callback;

    //        using var _ = logger.BeginScope(new Dictionary<string, object>
    //        {
    //            ["OrderId"] = cb.OrderId,
    //            ["CallbackResCode"] = cb.ResCode
    //        });

    //        // 1) پیدا کردن پرداخت
    //        var row = await repo.GetByOrderIdAsync(cb.OrderId, ct);
    //        if (row is null)
    //            throw new NotFoundException("پرداخت یافت نشد.");

    //        // 2) اگر قبلاً نهایی شده (idempotent)
    //        //if (row.IsFinal)
    //        //{
    //        //    logger.LogInformation("Payment already finalized. Ignoring duplicate callback.");
    //        //    return new CompleteSadadPaymentResultDto(row.Id, row.OrderId, true, cb.ResCode, "قبلاً تعیین تکلیف شده است.", null, null);
    //        //}

    //        // 3) تطبیق Token (با Hash) تا دستکاری نشه
    //        //var incomingHash = crypto.Sha256(cb.Token);
    //        //if (row.SadadTokenHash is null || !incomingHash.AsSpan().SequenceEqual(row.SadadTokenHash))
    //        //{
    //        //    logger.LogWarning("Token mismatch detected for OrderId={OrderId}", cb.OrderId);
    //        //    throw new LogicException("عدم تطابق توکن پرداخت (مشکوک به دستکاری).", AppErrorCodes.LogicError);
    //        //}

    //        // 4) ثبت Callback
    //        var okCb = await repo.SetCallbackAsync(row.Id, cb.ResCode, row.RowVersion, ct);
    //        if (!okCb) throw new LogicException("خطا در ثبت Callback (Concurrency).", AppErrorCodes.LogicError);

    //        // 5) اگر ResCode موفق نبود، Fail کن
    //        // طبق جدول: موفق معمولاً 0 است :contentReference[oaicite:18]{index=18}
    //        if (cb.ResCode != 0)
    //        {
    //            var row2 = await repo.GetByOrderIdAsync(cb.OrderId, ct)!;
    //            await repo.SetFailedAsync(row2!.Id, verifyResCode: null, row2.RowVersion, ct);

    //            logger.LogWarning("Callback not successful. ResCode={ResCode}", cb.ResCode);
    //            return new CompleteSadadPaymentResultDto(row.Id, row.OrderId, false, cb.ResCode, "پرداخت ناموفق/لغو شد.", null, null);
    //        }

    //        // 6) Verify (قطعیت پرداخت) :contentReference[oaicite:19]{index=19}
    //        var vr = await gateway.VerifyAsync(cb.Token, ct);

    //        // 7) اگر Verify موفق نبود → Fail
    //        //if (vr.ResCode != 0)
    //        //{
    //        //    var row3 = await repo.GetByOrderIdAsync(cb.OrderId, ct)!;
    //        //    await repo.SetFailedAsync(row3!.Id, vr.ResCode, row3.RowVersion, ct);

    //        //    logger.LogWarning("Verify failed. ResCode={ResCode}, Desc={Desc}", vr.ResCode, vr.Description);
    //        //    return new CompleteSadadPaymentResultDto(row.Id, row.OrderId, false, vr.ResCode, vr.Description ?? "Verify ناموفق بود.", null, null);
    //        //}


    //        //if (vr.OrderId != row.OrderId || vr.Amount != row.AmountRials)
    //        //{
    //        //    logger.LogError("Verify mismatch! Order/Amount mismatch. VerifyOrder={VOrder}, VerifyAmount={VAmt}",
    //        //        vr.OrderId, vr.Amount);

    //        //    var row4 = await repo.GetByOrderIdAsync(cb.OrderId, ct)!;
    //        //    await repo.SetFailedAsync(row4!.Id, vr.ResCode, row4.RowVersion, ct);

    //        //    throw new LogicException("عدم تطابق مبلغ/سفارش با Verify (مشکوک به دستکاری).", AppErrorCodes.LogicError);
    //        //}

    //        // 9) ثبت موفقیت نهایی (ref numbers)
    //        var row5 = await repo.GetByOrderIdAsync(cb.OrderId, ct)!;
    //        //var okFinal = await repo.SetVerifiedAsync(row5!.Id, new VerifyResultDb(
    //        //    VerifyResCode: vr.ResCode,
    //        //    VerifiedAmountRials: vr.Amount,
    //        //    RetrievalRefNo: vr.RetrivalRefNo,
    //        //    SystemTraceNo: vr.SystemTraceNo,
    //        //    OrderId: vr.OrderId
    //        //), row5.RowVersion, ct);

    //        //if (!okFinal) throw new LogicException("خطا در ثبت نتیجه نهایی (Concurrency).", AppErrorCodes.LogicError);

    //        //logger.LogInformation("Payment verified successfully. RetrievalRefNo={Ref}, Trace={Trace}",
    //        //    vr.RetrivalRefNo, vr.SystemTraceNo);
    //        var res = new CompleteSadadPaymentResultDto(OrderId:);


    //        return  Result<CompleteSadadPaymentResultDto>.Success(res);
    //    }


}
