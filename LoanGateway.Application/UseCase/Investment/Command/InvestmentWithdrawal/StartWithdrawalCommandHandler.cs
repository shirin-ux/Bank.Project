using Common;
using LoanService.Application.Contracts;
using LoanService.Domain.IRepository.Investment;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.UseCase.Investment.Command.InvestmentWithdrawal
{
//    public sealed class StartWithdrawalCommandHandler
//        : IRequestHandler<StartWithdrawalCommand, Result<StartWithdrawalResultDto>>
//    {
//       // private readonly IInvestmentAccountRepository _accounts;
//        private readonly IInvestmentWithdrawalRepository _withdrawals;
//        private readonly IInvestmentProvider _karizmah;
 


//        public StartWithdrawalCommandHandler(
//            IInvestmentAccountRepository accounts,
//            IInvestmentWithdrawalRepository withdrawals,
//            IInvestmentProvider karizmah
//)
//        {
//            _accounts = accounts;
//            _withdrawals = withdrawals;
//            _karizmah = karizmah;

//        }

//        public async Task<Result<StartWithdrawalResultDto>> Handle(
//            StartWithdrawalCommand command,
//            CancellationToken ct)
//        {
   
//            var account = await _accounts.GetByIdAsync(command.InvestmentAccountId, ct);

//            if (account is null)
//                return Result.Failure<StartWithdrawalResultDto>(Errors.InvestmentAccount.NotFound);

//            if (!account.IsActive)
//                return Result.Failure<StartWithdrawalResultDto>( Errors.InvestmentAccount.Inactive);

//            if (account.Provider != InvestmentProvider.Karizmah)
//                return Result.Failure<StartWithdrawalResultDto>( Errors.InvestmentAccount.UnsupportedProvider);

//            // 2) اعتبارسنجی مبلغ
//            if (command.Amount <= 0)
//                return Result.Failure<StartWithdrawalResultDto>(Errors.InvestmentWithdrawal.AmountMustBeGreaterThanZero);

//            var revokableAmount =
//                await _karizmah.GetRevokableAmountAsync(account.PolicyId, ct);

//            if (command.Amount > revokableAmount)
//                return Result.Failure<StartWithdrawalResultDto>(
//                    Errors.InvestmentWithdrawal.AmountExceedsRevokable(revokableAmount));

//            // 3) گرفتن TraceId
//            var traceId = await _karizmah.CreateTraceIdAsync(ct);

//            // 4) ساخت درخواست برای کاریزما
//            var phone = _currentUser.PhoneNumber ?? account.HolderPhoneNumber;

//            var providerRequest = new KarizmahDecreaseDirectRequest
//            {
//                Amount = command.Amount,
//                PolicyId = account.PolicyId,
//                BankAccountNumber = command.DestinationIban,
//                PhoneNumber = phone,
//                TraceId = traceId,
//                ReceiptDate = DateTime.UtcNow,
//                Description = command.Description
//            };

//            var providerResponse = await _karizmah.CreateDecreaseOrderAsync(providerRequest, ct);

//            if (!providerResponse.IsSuccess || providerResponse.Data is 0)
//            {
//                var firstError = providerResponse.ErrorMessages.FirstOrDefault();
//                return Result.Failure<StartWithdrawalResultDto>(
//                    Errors.InvestmentWithdrawal.ProviderError(
//                        firstError?.Code ?? -1,
//                        firstError?.Message ?? "خطا در سرویس برداشت کاریزما"));
//            }

//            long providerOrderId = providerResponse.Data;

//            // 5) محاسبه کارمزد و مبلغ نهایی (اینجا هاردکد؛ بعداً از تنظیمات/DB)
//            decimal fee = 100_000m; // TODO: از config یا جدول کارمزد
//            decimal finalAmount = command.Amount - fee;

//            // 6) ثبت در DB
//            var withdrawal = InvestmentWithdrawal.Create(
//                accountId: account.Id,
//                amount: command.Amount,
//                fee: fee,
//                destinationIban: command.DestinationIban,
//                providerOrderId: providerOrderId,
//                traceId: traceId);

//            _withdrawals.Add(withdrawal);
//            await _uow.SaveChangesAsync(ct);

//            // 7) DTO خروجی برای رفتن به صفحه OTP
//            var dto = new StartWithdrawalResultDto
//            {
//                WithdrawalId = withdrawal.Id,
//                ProviderOrderId = providerOrderId,
//                TraceId = traceId,
//                Amount = command.Amount,
//                Fee = fee,
//                FinalPayableAmount = finalAmount,
//                DestinationIban = command.DestinationIban,
//                MaskedPhoneNumber = MaskPhone(phone),
//                SettlementText = "۱ الی ۲ روز کاری"
//            };

//            return dto;
//        }

//        private static string MaskPhone(string? phone)
//        {
//            if (string.IsNullOrWhiteSpace(phone) || phone.Length < 4)
//                return string.Empty;

//            return new string('*', phone.Length - 4) + phone[^4..]; // ******4383
//        }
//    }
}
