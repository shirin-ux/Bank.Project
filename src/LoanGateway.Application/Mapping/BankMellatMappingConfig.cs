using Bank.Mellat.Provider.Dtos;
using LoanService.Application.UseCase.Loan.Command.GetCustomerBilling;
using LoanService.Application.UseCase.Loan.Command.GetCustomerCreditBalance;
using LoanService.Application.UseCase.Loan.Command.OtpRequest;
using LoanService.Application.UseCase.Loan.Query.GetInstallments;
using LoanService.Application.UseCase.Loan.Query.PayResponse;
using LoanService.Domain.Entities.Loan;
using Mapster;


namespace LoanService.Application.Mapping;

public static class BankMellatMappingConfig
{
    public static void RegisterMappings()
    {
        // 🔹 Mapping برای فایل قرارداد با وثیقه
        //TypeAdapterConfig<MellatFileUploadRes, GetCollateralContractFileResultDto>
        //    .NewConfig()
        //      .Map(d => d.MessageCode, s => s.messageCode)
        //      .Map(d => d.Message, s => s.message)
        //      .Map(dest => dest.ContractFile,
        //         src => src.ContractFile != null
        //                 ? Convert.ToBase64String(src.ContractFile)
        //                 : string.Empty);

        // 🔹 مثال: برای فایل قرارداد بدون وثیقه
        //TypeAdapterConfig<MellatFileUploadRes, GetContractFileResultDto>
        //    .NewConfig()
        //    .Map(dest => dest.ContractFile,
        //         src => src.ContractFile != null
        //                 ? Convert.ToBase64String(src.ContractFile)
        //                 : string.Empty);


        //TypeAdapterConfig<MellatPayResponseRes, GetPayResponseResultDto>
        //   .NewConfig()
        //   .Map(dest => dest.PayRequestStatus, src => src.payRequestStatus)
        //   .Map(dest => dest.PayContractInfo, src => src.payContractInfo);


        //TypeAdapterConfig<MellatPayResponseRes.PayRequestStatus, GetPayResponseResultDto.PayRequestStatusDto>
        //    .NewConfig();
        //   // .Map(dest => dest.ResponseCode, src => ToResponseCode(src.responseCode));


        TypeAdapterConfig<MellatPayResponseRes.PayContractInfo, GetPayResponseResultDto.PayContractInfoDto>
            .NewConfig()
            .Map(dest => dest.NationalCode, src => src.nationalCode)
            .Map(dest => dest.TraceCode, src => src.traceCode)
            .Map(dest => dest.ContractNo, src => src.contractNo)
            .Map(dest => dest.ContractDate, src => src.contractDate)
            .Map(dest => dest.LoanAmount, src => src.loanAmount)
            .Map(dest => dest.SumCost, src => src.sumCost)
            .Map(dest => dest.InstallmentCount, src => src.installmentCount)
            .Map(dest => dest.ContractFile, src => src.contractFile)
            .Map(dest => dest.CbTrackingCode, src => src.cbTrackingCode);


        TypeAdapterConfig<MellatCustomerCreditBalanceRes, GetCustomerCreditBalanceResultDto>
            .NewConfig()
            .Map(d => d.NationalCode, s => s.nationalCode)
            .Map(d => d.MessageCode, s => s.messageCode)
            .Map(d => d.Message, s => s.message)
            .Map(d => d.contractCreditList, s => s.ContractCreditList);

        TypeAdapterConfig<MellatCustomerCreditBalanceRes.contractCreditList, GetCustomerCreditBalanceResultDto.ContractCreditList>
            .NewConfig()
            .Map(d => d.ApprovalCode, s => s.approvalCode)
            .Map(d => d.ContractNumber, s => s.contractNumber)
            .Map(d => d.CreditBalance, s => s.creditBalance);

        TypeAdapterConfig<MellatOtpRes, OtpRequestResultDto>
         .NewConfig()
         .Map(d => d.MessageCode, s => s.messageCode)
         .Map(d => d.Message, s => s.message);

        //TypeAdapterConfig<MellatRepaymentRes, RepaymentRequestResultDto>
        //   .NewConfig()
        //   .Map(d => d.RepaymentDate, s => s.RepaymentDate)
        //   .Map(d => d.AccountNumber, s => s.AccountNumber)
        //   .Map(d => d.TrackNumber, s => s.TrackNumber)
        //   .Map(d => d.ContractNumber, s => s.ContractNumber)
        //   .Map(d => d.RepaymentAmount, s => s.RepaymentAmount)
        //   .Map(d => d.CustomerName, s => s.CustomerName)
        //   .Map(d => d.Message, s => s.Message)
        //   .Map(d => d.MessageCode, s => s.MessageCode);

        //TypeAdapterConfig<RepaymentRequestCommand, MellatRepaymentReq>
        //    .NewConfig()
        //    .Map(d => d.AccountNo, s => decimal.Parse(s.AccountNo))
        //    .Map(d => d.ContractNo, s => s.ContractNo)
        //    .Map(d => d.NationalCode, s => s.NationalCode)
        //    .Map(d => d.RepaymentAmount, s => s.RepaymentAmount)
        //    .Map(d => d.OtpCode, s => s.OtpCode);


        TypeAdapterConfig<MellatCustomerBillingRes, GetCustomerBillingResultDto>
           .NewConfig()
           .Map(d => d.Message, s => s.message)
           .Map(d => d.MessageCode, s => s.messageCode)
           .Map(d => d.Billings, s => s.billings);

        TypeAdapterConfig<MellatCustomerBillingRes.BillingItem, GetCustomerBillingResultDto.BillingItemDto>
            .NewConfig()
            .Map(d => d.CustomerName, s => s.customerName)
            .Map(d => d.PayDeadline, s => s.payDeadLine)
            .Map(d => d.TotalPurchase, s => s.totalPurchase)
            .Map(d => d.ContractNumber, s => s.contractNumber)
            .Map(d => d.DebtPayableInInstallments, s => s.debtPayableInInstallments)
            .Map(d => d.PayableAmount, s => s.payableAmount)
            .Map(d => d.IssueDate, s => s.issueDate)
            .Map(d => d.PeriodStartDate, s => s.perioadStartDate)
            .Map(d => d.PeriodEndDate, s => s.perioadEndDate)
            .Map(d => d.BillingNumber, s => s.billingNumber);




        //TypeAdapterConfig<TransferRegisterCommand, MellatTransferRegisterReq>
        //    .NewConfig()
        //    .Map(d => d.ApprovalCode, s => s.ApprovalCode)
        //    .Map(d => d.TransferDate, s => s.TransferDate)
        //    .Map(d => d.PayAmount, s => s.PayAmount)
        //    .Map(d => d.DestIban, s => s.DestIban)
        //    .Map(d => d.DestNationalId, s => s.DestNationalId)
        //    .Map(d => d.DestName, s => s.DestName)
        //    .Map(d => d.Description, s => s.Description)
        //    .Map(d => d.TransType, s => s.TransType)
        //    .Map(d => d.Details, s => s.Details);

        //TypeAdapterConfig<MellatTransferRegisterReq.TransferDetailDto, TransferRegisterCommand.TransferDetailItem>
        //    .NewConfig()
        //    .TwoWays();

        //TypeAdapterConfig<MellatTransferRegisterRes, TransferRegisterResultDto>
        //    .NewConfig()
        //    .Map(d => d.RegisterCode, s => s.RegisterCode)
        //    .Map(d => d.TransType, s => s.TransType)
        //    .Map(d => d.ContractsError, s => s.ContractsError)
        //    .Map(d => d.TransactionsError, s => s.TransactionsError)
        //    .Map(d => d.MessageCode, s => s.MessageCode)
        //    .Map(d => d.Message, s => s.Message);

        //TypeAdapterConfig<MellatTransferInquiryRes, TransferInquiryResultDto>
        //    .NewConfig()
        //    .Map(d => d.RegisterCode, s => s.RegisterCode)
        //    .Map(d => d.ApprovalCode, s => s.ApprovalCode)
        //    .Map(d => d.MessageCode, s => s.MessageCode)
        //    .Map(d => d.Message, s => s.Message)
        //    .Map(d => d.InquiryDetails, s => s.InquiryDetails);

        //TypeAdapterConfig<MellatTransferInquiryRes.TransferInquiryDetailDto, TransferInquiryResultDto.InquiryDetailDto>
        //    .NewConfig()
        //    //.Map(d => d.TransferStatus, s => s.TransferStatus switch
        //    //{
        //    //    1 => TransferStatus.Deleted,
        //    //    2 => TransferStatus.Returned,
        //    //    3 => TransferStatus.Sent,
        //    //    4 => TransferStatus.Unsendable,
        //    //    5 => TransferStatus.CanceledNoDebit,
        //    //    6 => TransferStatus.CanceledAndReturned,
        //    //    _ => TransferStatus.Registered
        //    //})
        //    .Map(d => d.PayAmount, s => s.PayAmount)
        //    .Map(d => d.DestIban, s => s.DestIban)
        //    .Map(d => d.DestName, s => s.DestName)
        //    .Map(d => d.DestNationalId, s => s.DestNationalId)
        //    .Map(d => d.TransType, s => s.TransType)
        //    .Map(d => d.Description, s => s.Description)
        //    .Map(d => d.SendDate, s => s.SendDate)
        //    .Map(d => d.SendTime, s => s.SendTime)
        //    .Map(d => d.ReturnDate, s => s.ReturnDate)
        //    .Map(d => d.ReturnTime, s => s.ReturnTime)
        //    .Map(d => d.DeleteDate, s => s.DeleteDate)
        //    .Map(d => d.DeleteTime, s => s.DeleteTime)
        //    .Map(d => d.TrackingNo, s => s.TrackingNo)
        //    .Map(d => d.ReturnReasonCode, s => s.ReturnReasonCode);

        TypeAdapterConfig<GetInstallmentsResultDto, InstallmentStatus>
            .NewConfig()
              .Map(dest => dest.ContractNumber, src => src.ContractNumber);

        TypeAdapterConfig<GetInstallmentsResultDto.InstallmentItemDto, InstallmentStatus>
            .NewConfig()
              .Map(dest => dest.InstallmentNo, src => src.InstallmentNo)
              .Map(dest => dest.DueDate, src => DateTimeOffset.FromUnixTimeSeconds(src.DueDate).DateTime)
              .Map(dest => dest.Amount, src => src.InstallmentAmount)
              .Map(dest => dest.PaidAmount, src => src.PaymentState == 1 ? src.InstallmentAmount : 0)
              .Map(dest => dest.Status, src => src.PaymentState == 1 ? "Paid" : "Pending");

        //TypeAdapterConfig< GetCustomerPurchaseDetailsCommand, MellatCustomerPurchaseDetailsReq>
        // .NewConfig()
        //      .Map(dest => dest.FromDate, src => DateConversion.ToBankIntDate(src.FromDate))
        //      .Map(dest => dest.ToDate, src => DateConversion.ToBankIntDate(src.ToDate))
        //      .Map(dest => dest.ContractNumber, src => src.ContractNumber)
        //      .Map(dest => dest.NationalCode, src => src.NationalCode);
    }
}

