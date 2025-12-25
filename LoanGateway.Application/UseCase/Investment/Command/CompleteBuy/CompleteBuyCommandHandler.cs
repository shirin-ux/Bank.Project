using Common;
using LoanService.Application.Contracts;
using LoanService.Domain.Entities.Investment;
using LoanService.Domain.Enum.Investment;
using LoanService.Domain.IRepository.Investment;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LoanService.Application.UseCase.Investment.Command.CompleteBuy;

public class CompleteBuyCommandHandler(
    IInvestmentProvider investmentProvider,
    IInvestmentPlanReadRepository investmentPlanReadRepository,
    IUserReadService userReadService,
    ILogger<CompleteBuyCommandHandler> logger)
    : IRequestHandler<CompleteBuyCommand, Result<CompleteBuyResult>>
{
    private readonly IInvestmentProvider _investmentProvider = investmentProvider;
    private readonly IInvestmentPlanReadRepository _investmentPlanReadRepository = investmentPlanReadRepository;
    private readonly IUserReadService _userReadService = userReadService;
    private readonly ILogger<CompleteBuyCommandHandler> _logger = logger;

    public async Task<Result<CompleteBuyResult>> Handle(CompleteBuyCommand request, CancellationToken cancellationToken)
    {

        var orderStatus = await _investmentProvider.GetOrderBuyByIdAsync(request.OrderId, cancellationToken);


        if (orderStatus.Status != statusType.FinalStatus || orderStatus.UliStatus != false)
        {
            _logger.LogWarning("Order {OrderId} هنوز در وضعیت نهایی نیست. Status: {Status}, UliStatus: {UliStatus}",
                request.OrderId, orderStatus.Status, orderStatus.UliStatus);

            return Result<CompleteBuyResult>.Failure(
                new Error(400, "بیمه‌نامه هنوز صادر نشده است. لطفاً بعداً تلاش کنید."));
        }


        var user = await _userReadService.GetUserByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result<CompleteBuyResult>.Failure(
                new Error(404, "کاربر یافت نشد."));
        }


        if (string.IsNullOrWhiteSpace(user.NationalCode) || string.IsNullOrWhiteSpace(user.BirthDate))
        {
            return Result<CompleteBuyResult>.Failure(
                new Error(400, "اطلاعات کاربر ناقص است. لطفاً پروفایل خود را تکمیل کنید."));
        }


        var existingAccount = await _investmentPlanReadRepository.GetByNationalCodeAndPlanAsync(
            user.NationalCode,
            request.PlanType,
            cancellationToken);

        if (existingAccount != null && existingAccount.ProviderPolicyId == request.ProviderPolicyId)
        {
            _logger.LogInformation("InvestmentAccount با PolicyId {PolicyId} قبلاً ایجاد شده است.", request.ProviderPolicyId);
            return Result<CompleteBuyResult>.Success(new CompleteBuyResult
            {
                InvestmentAccountId = existingAccount.Id,
                ProviderPolicyId = request.ProviderPolicyId,
                Message = "حساب سرمایه‌گذاری قبلاً ایجاد شده است."
            });
        }

        var plan = await _investmentPlanReadRepository.GetPlanAsync(request.PlanType.ToString(), cancellationToken);
        if (plan == null)
        {
            return Result<CompleteBuyResult>.Failure(
                new Error(404, "طرح سرمایه‌گذاری مورد نظر یافت نشد."));
        }


        var investmentAccount = InvestmentAccount.CreateNew(
            policyId: request.ProviderPolicyId,
            nationalCode: user.NationalCode,
            birthDate: user.BirthDate,
            planCode: request.PlanType,
            traceId: request.TraceId?.ToString() ?? Guid.NewGuid().ToString());

        await _investmentPlanReadRepository.AddAsync(investmentAccount, cancellationToken);

        _logger.LogInformation("InvestmentAccount با موفقیت ایجاد شد. AccountId: {AccountId}, PolicyId: {PolicyId}, OrderId: {OrderId}",
            investmentAccount.Id, request.ProviderPolicyId, request.OrderId);

        return Result<CompleteBuyResult>.Success(new CompleteBuyResult
        {
            InvestmentAccountId = investmentAccount.Id,
            ProviderPolicyId = request.ProviderPolicyId,
            Message = "حساب سرمایه‌گذاری با موفقیت ایجاد شد."
        });
    }
}

