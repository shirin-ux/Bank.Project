using LoanService.Application.UseCase.Loan.Command.CustomerInquiry;
using LoanService.Application.UseCase.Loan.Command.DepositRequest;
using LoanService.Application.UseCase.Loan.Command.GetCollateralContractFile;
using LoanService.Application.UseCase.Loan.Command.GetContractFile;
using LoanService.Application.UseCase.Loan.Command.GetCustomerBilling;
using LoanService.Application.UseCase.Loan.Command.GetCustomerCreditBalance;
using LoanService.Application.UseCase.Loan.Command.GetCustomerPurchaseDetails;
using LoanService.Application.UseCase.Loan.Command.OtpRequest;
using LoanService.Application.UseCase.Loan.Command.RepaymentReques;
using LoanService.Application.UseCase.Loan.Command.SubmitPayRequest;
using LoanService.Application.UseCase.Loan.Command.TransferRegister;
using LoanService.Application.UseCase.Loan.Query.CustomerInquiryStatus;
using LoanService.Application.UseCase.Loan.Query.GetInstallments;
using LoanService.Application.UseCase.Loan.Query.PayResponse;
using LoanService.Application.UseCase.Loan.Query.ReturnTransferReport;
using LoanService.Application.UseCase.Loan.Query.TransferInquiry;

namespace LoanService.Application.Contracts;

public interface IProvider : IProviderBase
{

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
