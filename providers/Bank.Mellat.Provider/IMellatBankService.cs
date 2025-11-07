using Bank.Mellat.Provider.Dtos;
namespace Bank.Mellat.Provider;

public interface IMellatBankService
{

    Task<MellatInquiryRegisterRes> RegisterInquiryAsync(MellatInquiryRegisterReq req, CancellationToken ct);
    Task<MellatInquiryResultRes> GetInquiryResultAsync(string ticketId, CancellationToken ct);


    Task<MellatFileUploadRes> UploadContractFileAsync(MellatFileUploadReq req, CancellationToken ct);
    Task<MellatFileUploadRes> UploadCollateralFileAsync(MellatContractWithCollateralReq req, CancellationToken ct);
    Task<MellatCustomerCreditBalanceRes> GetCustomerCreditBalanceAsync(MellatCustomerCreditBalanceReq request, CancellationToken ct);
    Task<string> GetAccessTokenAsync(CancellationToken ct);
    Task<MellatPayResponseRes> GetPayResponseAsync(MellatPayReq req, CancellationToken ct);
    Task<MellatInstallmentsRes> GetInstallmentsAsync(MellatInstallmentsReq req, CancellationToken ct);
    Task<MellatOtpRes> RequestOtpAsync(MellatOtpReq req, CancellationToken ct);
    Task<MellatDepositRes> RequestDepositAsync(MellatDepositReq req, CancellationToken ct);

    Task<MellatRepaymentRes> RequestRepaymentAsync(MellatRepaymentReq req, CancellationToken ct);

    Task<MellatCustomerBillingRes> GetCustomerBillingAsync(MellatCustomerBillingReq req, CancellationToken ct);

    Task<MellatCustomerPurchaseDetailsRes> CustomerPurchaseDetailsAsync(MellatCustomerPurchaseDetailsReq req, CancellationToken ct);
    Task<MellatTransferRegisterRes> RegisterTransferAsync(MellatTransferRegisterReq req, CancellationToken ct);
    Task<MellatTransferInquiryRes> GetTransferInquiryAsync(MellatTransferInquiryReq req, CancellationToken ct );
    Task<MellatSubmitPayRequestRes> SubmitPayRequestAsync(MellatSubmitPayRequestReq req, CancellationToken ct);
    Task<MellatReturnTransferReportRes> GetReturnTransferReportAsync(MellatReturnTransferReportReq req, CancellationToken ct);

}
