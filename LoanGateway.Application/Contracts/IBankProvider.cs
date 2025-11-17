
using Common;
using LoanService.Application.UseCase.Command.CustomerInquiry;
using LoanService.Application.UseCase.Command.DepositRequest;
using LoanService.Application.UseCase.Command.GetCollateralContractFile;
using LoanService.Application.UseCase.Command.GetContractFile;
using LoanService.Application.UseCase.Command.GetCustomerBilling;
using LoanService.Application.UseCase.Command.GetCustomerCreditBalance;
using LoanService.Application.UseCase.Command.GetCustomerPurchaseDetails;
using LoanService.Application.UseCase.Command.OtpRequest;
using LoanService.Application.UseCase.Command.RepaymentReques;
using LoanService.Application.UseCase.Command.SubmitPayRequest;
using LoanService.Application.UseCase.Command.TransferRegister;
using LoanService.Application.UseCase.Query.CustomerInquiryStatus;
using LoanService.Application.UseCase.Query.GetInstallments;
using LoanService.Application.UseCase.Query.PayResponse;
using LoanService.Application.UseCase.Query.ReturnTransferReport;
using LoanService.Application.UseCase.Query.TransferInquiry;
using LoanService.Domain.Enum.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Application.Contracts
{
    public interface IBankProvider
    {
        BankProviderType ProviderType { get; }
        Task<CustomerInquiryResultDto> CustomerInquiryAsync(CustomerInquiryCommand cmd, CancellationToken ct);
        Task<CustomerInquiryStatusResultDto> GetCustomerInquiryStatusAsync(string requestId, CancellationToken ct);
        Task<GetContractFileResultDto> GetContractFileAsync(GetContractFileCommand cmd, CancellationToken ct);
        Task<GetCollateralContractFileResultDto> GetCollateralContractFileAsync(GetCollateralContractFileCommand cmd, CancellationToken ct);
        Task<GetPayResponseResultDto> GetPayResponseAsync(string payRequestId, CancellationToken ct);
        Task<GetInstallmentsResultDto> GetInstallmentsAsync(string nationalCode, string contractNumber, CancellationToken ct);
        Task<SubmitPayRequestResultDto> SubmitPayRequestAsync(SubmitPayRequestCommand cmd, CancellationToken ct);

        Task<GetCustomerCreditBalanceResultDto> GetCustomerCreditBalanceAsync(GetCustomerCreditBalanceCommand cmd, CancellationToken ct);
        Task<OtpRequestResultDto> RequestOtpAsync(OtpRequestCommand cmd, CancellationToken ct);
        Task<DepositRequestResultDto> DepositRequestAsync(DepositRequestCommand cmd, CancellationToken ct);
        Task<RepaymentRequestResultDto> RepaymentRequestAsync(RepaymentRequestCommand cmd, CancellationToken ct);
        Task<GetCustomerBillingResultDto> GetCustomerBillingAsync(GetCustomerBillingCommand cmd, CancellationToken ct);
        Task<GetCustomerPurchaseDetailsResultDto> GetCustomerPurchaseDetailsAsync(GetCustomerPurchaseDetailsCommand cmd, CancellationToken ct);

        Task<TransferRegisterResultDto> RegisterTransferAsync(TransferRegisterCommand cmd, CancellationToken ct);
        Task<TransferInquiryResultDto> TransferInquiryAsync(TransferInquiryQuery q, CancellationToken ct);
        Task<ReturnTransferReportResultDto> GetReturnTransferReportAsync(ReturnTransferReportCommand q, CancellationToken ct);
    }
}
