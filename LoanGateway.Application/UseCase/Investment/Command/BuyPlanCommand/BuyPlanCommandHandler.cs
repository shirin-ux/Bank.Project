using Common;
using LoanService.Application.Contracts;
using LoanService.Application.Exceptions;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.Exceptions;
using LoanService.Domain.IRepository.Investment;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LoanService.Application.UseCase.Investment.Command.BuyPlanCommand;

public sealed class BuyPlanCommandHandler
    : IRequestHandler<BuyPlanCommand, Result<BuyPlanResultDto>>
{
    private readonly IInvestmentPlanReadRepository _investmentPlanReadRepository;

    private readonly IInvestmentProvider _investmentProvider;
    private readonly ILogger<BuyPlanCommandHandler> _logger;

    public BuyPlanCommandHandler(
        IInvestmentPlanReadRepository readRepo,
        IInvestmentProvider investmentProvider,
        ILogger<BuyPlanCommandHandler> logger)
    {
        _investmentPlanReadRepository =readRepo;
        _investmentProvider = investmentProvider;
        _logger = logger;
    }

    public async Task<Result<BuyPlanResultDto>> Handle(BuyPlanCommand cmd,CancellationToken ct)
    {
        var plan = await _investmentPlanReadRepository.GetPlanAsync(cmd.PlanType.ToString(), ct);
        if (plan is null)
            throw new NotFoundException("طرح سرمایه‌گذاری مورد نظر یافت نشد.");

        if (cmd.AmountRial < plan.MinAmount)
            throw new LogicException(
                $"حداقل مبلغ سرمایه‌گذاری در این طرح {plan.MinAmount:N0} ریال است.");


        var nationalCode = cmd.NationalCode;
        if (string.IsNullOrWhiteSpace(nationalCode))
            throw new LogicException("کد ملی کاربر نامشخص است.");


        var priceInfo = await _investmentProvider.GetCurrentPriceAsync(cmd.PlanType, ct);

        var gramPrice = priceInfo.CurrentPrice;


        var gramsRaw = (decimal)cmd.AmountRial / gramPrice;
        var grams = Math.Floor(gramsRaw * 1000m) / 1000m;

        if (grams <= 0)
            throw new LogicException("مبلغ وارد شده برای خرید طلا ناکافی است.");


        var orderRes = await _investmentProvider.CreatePolicyAndBuyAsync(cmd, ct);

 
        var account = InvestmentAccount.CreateNew(
           
            policyId: orderRes.PolicyId,
            nationalCode: orderRes.NationalCode,
            birthDate:orderRes.BirthDate,  
            planCode: plan.PlanType,
            traceId: orderRes.TraceId.ToString()
        );


        await _investmentPlanReadRepository.AddAsync(account, ct);
        var resultDto = new BuyPlanResultDto
        {
            PlanType = orderRes.PlanType,
            AmountRial = orderRes.AmountRial,
            GramPrice = gramPrice,
            Grams = grams,
            DailyChangePercent = priceInfo.DailyChangePercent,
            PolicyId = orderRes.PolicyId,
            OrderId = orderRes.OrderId,
            TraceId = orderRes.TraceId,
        };

        return Result<BuyPlanResultDto>.Success(resultDto);
    }

}
