using Bank.Mellat.Provider;
using Bank.Mellat.Provider.Dtos;
using LoanService.Application.Contracts;
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
using LoanService.Domain.Enum;
using LoanService.Domain.Enum.Loan;
using LoanService.Infrastructure.Extention;
using MapsterMapper;
using System.Globalization;
using static Bank.Mellat.Provider.Dtos.MellatPayResponseRes;


namespace Bank.Mellat.Infrastructure.Services
{
    public class MellatBankProvider(IMellatBankService client,
        IMapper mapper, IContractFileStorage contractFileStorage) : IProvider
    {
        private readonly IMellatBankService _client = client;
        private readonly IMapper _mapper = mapper;
        private readonly IContractFileStorage _contractFileStorage = contractFileStorage;
        public ProviderType ProviderType => ProviderType.Mellat;

        public async Task<CustomerInquiryResultDto> CustomerInquiryAsync(CustomerInquiryCommand cmd, CancellationToken ct)
        {
            try
            {
                //var mellatReq = _mapper.Map<MellatInquiryRegisterReq>(cmd);
                var mellatReq = new MellatInquiryRegisterReq
                {
                    birthDate = cmd.BirthDate,
                    mobileNo = cmd.MobileNo,
                    nationalCode = cmd.NationalCode,
                    requestAmount = cmd.RequestAmount.HasValue ? ((decimal)cmd.RequestAmount.Value).ToString() : null,
                    approvalCode = cmd.ApprovalCode,
                    cbTrackingCode = cmd.CbTrackingCode,
                    //configType = cmd.ConfigType,
                    postalCode = cmd.PostalCode

                };


                var response = await client.RegisterInquiryAsync(mellatReq, ct);
                var result = new CustomerInquiryResultDto
                {
                    MessageCode = response.messageCode,
                    Message = response.message,
                    RequestId = response.requestId
                };

                //var result = _mapper.Map<CustomerInquiryResultDto>(response);

                return result;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public async Task<DepositRequestResultDto> DepositRequestAsync(DepositRequestCommand cmd, CancellationToken ct)
        {
            var mellatReq = new MellatDepositReq
            {
                transactionDesc = cmd.TransactionDesc,
                buyerNationalCode = cmd.BuyerNationalCode,
                contractNumber = cmd.ContractNumber,
                depositType = (short)cmd.DepositType,
                otpCode = cmd.OtpCode,
                payAmount = cmd.PayAmount,
                sellerAccountNo = decimal.Parse(cmd.SellerAccountNo, NumberStyles.None, CultureInfo.InvariantCulture),
                sellerNationalCode = cmd.SellerNationalCode

            };

            var response = await client.RequestDepositAsync(mellatReq, ct);

            var result = new DepositRequestResultDto
            {
                TransactionNumber = response.transactionNumber,
                Message = response.message,
                MessageCode = response.messageCode
            };

            return result;
        }

        public async Task<GetCustomerBillingResultDto> GetCustomerBillingAsync(GetCustomerBillingCommand cmd, CancellationToken ct)
        {
            var mellatReq = new MellatCustomerBillingReq
            {
                contractNumber = decimal.Parse(cmd.ContractNumber, NumberStyles.None, CultureInfo.InvariantCulture),
                billingNumber = cmd.BillingNumber,
                nationalCode = cmd.NationalCode
            };

            var response = await client.GetCustomerBillingAsync(mellatReq, ct);

            var result = new GetCustomerBillingResultDto
            {
                Billings = response.billings.Select(x => new GetCustomerBillingResultDto.BillingItemDto
                {
                    BillingNumber = x.billingNumber,
                    ContractNumber = x.contractNumber,
                    CustomerName = x.customerName,
                    DebtPayableInInstallments = x.debtPayableInInstallments,
                    IssueDate = x.issueDate,
                    PayableAmount = x.payableAmount,
                    PayDeadline = x.payDeadLine,
                    TotalPurchase = x.totalPurchase

                }).ToList(),
                Message = response.message,
                MessageCode = response.messageCode

            };


            return result;
        }

        public async Task<GetCustomerCreditBalanceResultDto> GetCustomerCreditBalanceAsync(GetCustomerCreditBalanceCommand cmd, CancellationToken ct)
        {
            var mellatReq = new MellatCustomerCreditBalanceReq
            {
                contractNumber = decimal.Parse(cmd.ContractNumber, NumberStyles.None, CultureInfo.InvariantCulture),
                nationalCode = cmd.NationalCode
            };
            var response = await client.GetCustomerCreditBalanceAsync(mellatReq, ct);
            if (response is null)
            {
                return new GetCustomerCreditBalanceResultDto
                {
                    NationalCode = cmd.NationalCode,
                    contractCreditList = new List<GetCustomerCreditBalanceResultDto.ContractCreditList>()
                };
            }
            var items = response.ContractCreditList ?? Array.Empty<MellatCustomerCreditBalanceRes.contractCreditList>();


            var result = new GetCustomerCreditBalanceResultDto
            {
                NationalCode = response.nationalCode ?? cmd.NationalCode,
                contractCreditList = items.Select(x => new GetCustomerCreditBalanceResultDto.ContractCreditList
                {
                    ApprovalCode = x.approvalCode,
                    ContractNumber = x.contractNumber,
                    CreditBalance = x.creditBalance
                }).ToList(),
                Message = response.message,
                MessageCode = response.messageCode
            };

            return result;
        }


        public async Task<CustomerInquiryStatusResultDto> GetCustomerInquiryStatusAsync(string requestId, CancellationToken ct)
        {
            var response = await _client.GetInquiryResultAsync(requestId, ct);

            var result = new CustomerInquiryStatusResultDto
            {
                RequestId = requestId,
                Allowed = response.Result.allowed,
                Ics = response.Result.ics,
                IcsGrade = ParseIcsGrade(response.Result.icsGrade),
                Gender = response.Result.Gender,
                MaxApprovedAmount = response.Result.maxApprovedAmount,
                RequestExpireDate = response.Result.requestExpireDate,
                PostalCodeStatus = response.Result.postalCode,
                StatusList = response.Result.statusList?
                             .Select(s => new CustomerInquiryStatusResultDto.StatusItemDto(
                                 s.responseCode,
                                 s.responseStatus
                             ))
                             .ToList()
            };
            return result;
        }
        private static Grade ParseIcsGrade(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Grade.Unknown;

            return Enum.TryParse<Grade>(value, ignoreCase: true, out var grade)
                ? grade
                : Grade.Unknown;
        }

        public async Task<GetCollateralContractFileResultDto> GetCollateralContractFileAsync(GetCollateralContractFileCommand cmd, CancellationToken ct)
        {
            //var mellatReq = _mapper.Map<MellatContractWithCollateralReq>(cmd);
            var mellatReq = new MellatContractWithCollateralReq
            {
                collateralIssuer = cmd.CollateralIssuer,
                address = cmd.Address,
                approvalCode = cmd.ApprovalCode,
                birthDate = cmd.BirthDate,
                chequeSerial = cmd.ChequeSerial,
                collateralAmount = cmd.CollateralAmount,
                collateralDate = cmd.CollateralDate,
                collateralNo = cmd.CollateralNo,
                collateralType = CollateralTypeExtensions.ToMellatValue(cmd.CollateralType),
                guarantorNC = cmd.GuarantorNC,
                nationalCode = cmd.NationalCode,
                installmentCount = cmd.InstallmentCount,
                loanAmount = cmd.LoanAmount,
                mobileNumber = cmd.MobileNumber,
                phoneNumber = cmd.PhoneNumber,
                postalCode = cmd.PostalCode
            };
            int? messageCode = null;
            var response = await client.UploadCollateralFileAsync(mellatReq, ct);
            if (!string.IsNullOrWhiteSpace(response.messageCode) && int.TryParse(response.messageCode, out var parsed))
            {
                messageCode = parsed;
            }
            var result = new GetCollateralContractFileResultDto
            {
                ContractFile = response.fileTemplate,
                ContractNumber = Convert.ToDecimal(response.contractNumber),
                Message = response.message,
                MessageCode = messageCode
            };
            //var result = _mapper.Map<GetCollateralContractFileResultDto>(response);

            return result;
        }

        public async Task<GetContractFileResultDto> GetContractFileAsync(GetContractFileCommand cmd, CancellationToken ct)
        {
            //var mellatReq = _mapper.Map<MellatFileUploadReq>(cmd);
            var mellatReq = new MellatFileUploadReq
            {
                address = cmd.Address,
                approvalCode = cmd.ApprovalCode,
                birthDate = cmd.BirthDate,
                nationalCode = cmd.NationalCode,
                installmentCount = cmd.InstallmentCount,
                loanAmount = cmd.LoanAmount,
                mobileNumber = cmd.MobileNumber,
                phoneNumber = cmd.PhoneNumber,
                postalCode = cmd.PostalCode
            };
            int? messageCode = null;
            var response = await client.UploadContractFileAsync(mellatReq, ct);
            if (!string.IsNullOrWhiteSpace(response.messageCode) && int.TryParse(response.messageCode, out var parsed))
            {
                messageCode = parsed;
            }
            var result = new GetContractFileResultDto
            {
                ContractFile = response.fileTemplate,
                ContractNumber = Convert.ToDecimal(response.contractNumber),
                Message = response.message,
                MessageCode = messageCode
            };
            //var result = _mapper.Map<GetContractFileResultDto>(response);

            return result;
        }

        public async Task<GetPayResponseResultDto> GetPayResponseAsync(string payRequestId, CancellationToken ct)
        {
            var response = await _client.GetPayResponseAsync(new MellatPayReq { PayRequestId = payRequestId }, ct);

            var contract = response.payContractInfo ?? new PayContractInfo();

            var status = response.payRequestStatus ?? new PayRequestStatus { responseCode = 0 };

            return new GetPayResponseResultDto
            {
                PayContractInfo = new GetPayResponseResultDto.PayContractInfoDto
                {
                    CbTrackingCode = contract.cbTrackingCode,
                    ContractDate = contract.contractDate,
                    ContractFile = contract.contractFile,
                    ContractNo = contract.contractNo,
                    InstallmentCount = contract.installmentCount,
                    LoanAmount = contract.loanAmount,
                    NationalCode = contract.nationalCode,
                    SumCost = contract.sumCost,
                    TraceCode = contract.traceCode
                },

                MessageCode = Convert.ToInt32(status.responseCode),
                Message = status.responseMessage

            };
        }

        public async Task<ReturnTransferReportResultDto> GetReturnTransferReportAsync(ReturnTransferReportCommand cmd, CancellationToken ct)
        {
            var mellatReq = new MellatReturnTransferReportReq
            {
                fromId = cmd.FromId,
                returnDate = cmd.ReturnDate
            };

            var response = await client.GetReturnTransferReportAsync(mellatReq, ct);

            var result = new ReturnTransferReportResultDto
            {

                ReturnedTransfers = response.Result.returnedTransfers.Select(x => new ReturnTransferReportResultDto.ReturnedTransferDto
                {
                    ApprovalId = x.approvalId,
                    DestBankCode = x.destBankCode,
                    DestIban = x.sourceIban,
                    DestName = x.destName,
                    NoSendDate = x.noSendDate,
                    PayAmount = x.payAmount,
                    ReasonCode = x.reasonCode,
                    RegisterCode = x.registerCode,
                    ReturnDate = x.returnDate,
                    ReturnReasonCode = x.returnReasonCode,
                    ReturnReasonDesc = x.returnReasonDesc,
                    ReturnTime = x.returnTime,
                    RowId = x.rowId,
                    SendDate = x.sendDate,
                    SendTime = x.sendTime,
                    SourceIban = x.sourceIban,
                    TrackingNo = x.trackingNO,
                    TransferDate = x.transferDate,
                    TransferStatus = (ReturnTransferReportResultDto.TransferStatus)x.transferStatus

                }).ToList(),
            };

            return result;
        }
        public async Task<GetCustomerPurchaseDetailsResultDto> GetCustomerPurchaseDetailsAsync(GetCustomerPurchaseDetailsCommand cmd, CancellationToken ct)
        {
            var mellatReq = new MellatCustomerPurchaseDetailsReq
            {
                contractNumber = decimal.Parse(cmd.ContractNumber, NumberStyles.None, CultureInfo.InvariantCulture),
                nationalCode = cmd.NationalCode,
                toDate = Convert.ToInt32(cmd.ToDate),
                fromDate = Convert.ToInt32(cmd.FromDate)
            };

            var response = await client.CustomerPurchaseDetailsAsync(mellatReq, ct);

            var result = new GetCustomerPurchaseDetailsResultDto
            {
                NationalCode = response.nationalCode,
                ContractAmount = response.contractAmount,
                ContractDetails = response.ContractDetails.Select(x => new GetCustomerPurchaseDetailsResultDto.ContractDetailDto
                {
                    PayAccNumber = x.payAccNumber,
                    DocDate = x.docDate.ToString(),
                    SellerName = x.sellerName,
                    SellerNationalCode = x.sellerNationalCode,
                    SellerPaiedAmount = x.sellerPaiedAmount,
                    TransactionDate = x.transactionDate.ToString(),
                    TransactionNumber = x.transactionNumber,
                    UsedCreditAmount = x.usedCreditAmount

                }).ToList(),
                ContractNumber = response.contractNumber,
                CustomerName = response.customerName,
                LoanPaiedAmount = response.loanPaiedAmount,
                LoanTypeCode = response.loanTypeCode,
                LoanTypeDesc = response.loanTypeDesc,
                Message = response.message,
                MessageCode = response.messageCode,

            };

            return result;
        }

        public async Task<GetInstallmentsResultDto> GetInstallmentsAsync(string nationalCode, string contractNumber, CancellationToken ct)
        {
            var response = await _client.GetInstallmentsAsync(new MellatInstallmentsReq { NationalCode = nationalCode, ContractNumber = decimal.Parse(contractNumber, NumberStyles.None, CultureInfo.InvariantCulture) }, ct);
            var result = new GetInstallmentsResultDto
            {
                ContractNumber = response.ContractNumber,
                ContractDesc = response.ContractDesc,
                DebtAmount = response.DebtAmount,
                DiscountedDebtAmount = response.DiscountedDebtAmount,
                EarlierInstallmentDate = response.EarlierInstallmentDate,
                InstallmentAmount = response.InstallmentAmount,
                Installments = response.Installment.Select(x => new GetInstallmentsResultDto.InstallmentItemDto
                {
                    InstallmentAmount = x.InstallmentAmount,
                    CapitalAmount = x.CapitalAmount,
                    DueDate = x.DueDate,
                    DueState = x.DueState,
                    InstallmentNo = x.InstallmentNo,
                    InterestAmount = x.InterestAmount,
                    PaymentState = x.PaymentState,
                    PenaltyAmount = x.PenaltyAmount
                }).ToList()
            };
            return result;
        }


        public async Task<TransferRegisterResultDto> RegisterTransferAsync(TransferRegisterCommand cmd, CancellationToken ct)
        {
            var mellatReq = new MellatTransferRegisterReq
            {
                transType = cmd.TransType,
                approvalCode = cmd.ApprovalCode,
                description = cmd.Description,
                destIban = cmd.DestIban,
                destName = cmd.DestName,
                destNationalId = cmd.DestNationalId,
                details = cmd.Details.Select(x => new MellatTransferRegisterReq.contractDetails
                {
                    amount = x.Amount,
                    referenceNo = x.ReferenceNo
                }).ToList()
            };

            var response = await client.RegisterTransferAsync(mellatReq, ct);

            var result = new TransferRegisterResultDto
            {

                Message = response.message,
                MessageCode = response.messageCode,
                RegisterCode = response.registerCode,
                TransactionsError = response.transactionsError,
                TransType = response.transType
            };

            return result;
        }

        public async Task<RepaymentRequestResultDto> RepaymentRequestAsync(RepaymentRequestCommand cmd, CancellationToken ct)
        {
            var mellatReq = new MellatRepaymentReq
            {
                accountNo = decimal.Parse(cmd.AccountNo, NumberStyles.None, CultureInfo.InvariantCulture),
                contractNo = decimal.Parse(cmd.ContractNo, NumberStyles.None, CultureInfo.InvariantCulture),
                nationalCode = cmd.NationalCode,
                otpCode = cmd.OtpCode,
                repaymentAmount = cmd.RepaymentAmount
            };

            var response = await client.RepaymentRequestAsync(mellatReq, ct);

            var result = new RepaymentRequestResultDto
            {
                RepaymentAmount = response.repaymentAmount,
                AccountNumber = response.accountNumber,
                ContractNumber = response.contractNumber,
                CustomerName = response.customerName,
                Message = response.message,
                MessageCode = response.messageCode,
                RepaymentDate = response.repaymentDate,
                TrackNumber = response.trackNumber
            };

            return result;
        }

        public async Task<OtpRequestResultDto> RequestOtpAsync(OtpRequestCommand cmd, CancellationToken ct)
        {
            var mellatReq = new MellatOtpReq
            {
                accountNumber = cmd.AccountNumber,
                serviceType = (short)cmd.ServiceType,
                payAmount = cmd.PayAmount,
                contractNumber = cmd.ContractNumber,
                nationalCode = cmd.NationalCode
            };

            var response = await client.RequestOtpAsync(mellatReq, ct);

            var result = new OtpRequestResultDto
            {
                Message = response.message,
                MessageCode = Convert.ToInt32(response.messageCode)
            };

            return result;
        }

        public async Task<SubmitPayRequestResultDto> SubmitPayRequestAsync(SubmitPayRequestCommand cmd, CancellationToken ct)
        {
            var path = cmd.ContractPath;

            var fileBytes = await _contractFileStorage.ReadAsync(path, ct);
            string base64Contract = Convert.ToBase64String(fileBytes);

            var mellatReq = new MellatSubmitPayRequestReq
            {
                contractFile = base64Contract,
                contractNumber = cmd.ContractNumber,
                requestAmount = cmd.RequestAmount

            };

            var response = await client.SubmitPayRequestAsync(mellatReq, ct);

            var result = new SubmitPayRequestResultDto
            {
                PayRequestId = response.payRequestId,
                Message = response.responseMessage,
                MessageCode = Convert.ToInt32(response.responseCode)
            };

            return result;
        }

        public async Task<TransferInquiryResultDto> TransferInquiryAsync(TransferInquiryQuery q, CancellationToken ct)
        {
            var response = await _client.GetTransferInquiryAsync(new MellatTransferInquiryReq { RegisterCode = q.RegisterCode }, ct);
            var result = new TransferInquiryResultDto
            {
                ApprovalCode = response.Result.aprovalCode,
                InquiryDetails = response.Result.inquiryDetails.Select(x => new TransferInquiryResultDto.InquiryDetailDto
                {
                    DeleteDate = x.deleteDate,
                    DeleteTime = x.deleteDate,
                    Description = x.description,
                    DestIban = x.destIban,
                    DestName = x.destName,
                    DestNationalId = x.destNationalId,
                    PayAmount = x.payAmount,
                    ReturnDate = x.returnDate,
                    ReturnReasonCode = x.returnReasonCode,
                    ReturnTime = x.returnTime,
                    SendDate = x.sendDate,
                    SendTime = x.sendTime,
                    TrackingNo = x.trackingNo,
                    //  transferStatus = (TransferStatus)x.transferStatus,
                    TransType = x.transType

                }).ToList()
            };
            return result;
        }
    }
}

