using LoanService.Application.Contracts;
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
using LoanService.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanService.Infrastructure.Services
{
   public class KarizmahInvestmentProvider : IProviderBase
    {
        public ProviderType ProviderType => throw new NotImplementedException();

      
    }
}
