using Common;
using LoanService.Application.Contracts;
using LoanService.Application.Exceptions;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.IRepository.Investment;
using MediatR;
using Microsoft.Extensions.Logging;
using Shahkar.Provider;

namespace LoanService.Application.UseCase.Investment.Command.BuyPlanCommand;

public sealed class BuyPlanCommandHandler
    : IRequestHandler<BuyPlanCommand, Result<BuyPlanResultDto>>
{
    private readonly IInvestmentPlanReadRepository _investmentPlanReadRepository;
    private readonly IUserContext _userContext;
    private readonly IUserApiClient _userApi;
    private readonly IInvestmentProvider _investmentProvider;
    private readonly ILogger<BuyPlanCommandHandler> _logger;

    private readonly IShahkarService _shahkarService;

    public BuyPlanCommandHandler(
        IInvestmentPlanReadRepository readRepo,
        IInvestmentProvider investmentProvider,
        ILogger<BuyPlanCommandHandler> logger,
        IUserApiClient userApi,
        IUserContext userContext,
        IShahkarService shahkarService)
    {
        _investmentPlanReadRepository = readRepo;
        _investmentProvider = investmentProvider;
        _logger = logger;
        _userContext = userContext;
        _userApi = userApi;
        _shahkarService = shahkarService;
    }

    public async Task<Result<BuyPlanResultDto>> Handle(BuyPlanCommand cmd, CancellationToken ct)
    {
        if (!_userContext.IsAuthenticated)
            throw new UnauthorizedAccessException("کاربر احراز هویت نشده");

        var plan = await _investmentPlanReadRepository.GetPlanAsync(cmd.PlanType.ToString(), ct);
        if (plan is null)
            throw new NotFoundException("طرح سرمایه‌گذاری مورد نظر یافت نشد.");

        _logger.LogInformation("Starting GiftGoldCommand for NationalCode {NationalCode}", cmd.NationalCode);


        var user = await _userApi.GetUserByNationalCodeAsync(new UserRequest { nationalCode = cmd.NationalCode }, ct);
        

        if (user is null)
            throw new NotFoundException($"کاربر با کد ملی {cmd.NationalCode} یافت نشد.");



        //var update = await _userApi.UpdateUserAsync(new UserIdRequest { PostalCode = user.PostalCode,UserId=user.UserId}, ct);
        //var priceInfo = await _investmentProvider.GetCurrentPriceAsync(cmd.PlanType, ct);

        //var gramPrice = priceInfo.CurrentPrice;


        //var gramsRaw = (decimal)cmd.AmountRial / gramPrice;
        //var grams = Math.Floor(gramsRaw * 1000m) / 1000m;

        //if (grams <= 0)
        //    throw new LogicException("مبلغ وارد شده برای خرید طلا ناکافی است.");


        var orderRes = await _investmentProvider.CreatePolicyAndBuyAsync(cmd, user.PostalCode, user.BirthDate, ct);


        var account = InvestmentAccount.CreateNew(

            policyId: orderRes.ProviderPolicyId,
            nationalCode: orderRes.NationalCode,
            birthDate: orderRes.BirthDate,
            planCode: plan.PlanType,
            traceId: orderRes.TraceId.ToString()
        );


        await _investmentPlanReadRepository.AddAsync(account, ct);
        var resultDto = new BuyPlanResultDto
        {
            PlanType = orderRes.PlanType,
            AmountRial = orderRes.AmountRial,
            ProviderPolicyId = orderRes.ProviderPolicyId,
            OrderId = orderRes.OrderId,
            TraceId = orderRes.TraceId,
        };

        return Result<BuyPlanResultDto>.Success(resultDto);
    }

}
