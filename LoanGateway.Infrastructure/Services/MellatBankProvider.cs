using Bank.Mellat.Provider;
using Bank.Mellat.Provider.Dtos;
using Common;
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
using LoanService.Domain.Entities;
using LoanService.Domain.Enum;
using LoanService.Infrastructure.Extention;
using MapsterMapper;
using System.Security.Cryptography.Pkcs;
using static LoanService.Application.UseCase.Query.CustomerInquiryStatus.CustomerInquiryStatusResultDto;


namespace Bank.Mellat.Infrastructure.Services
{
    public class MellatBankProvider(IMellatBankService client, IMapper mapper) : IBankProvider
    {
        private readonly IMellatBankService _client = client;
        private readonly IMapper _mapper = mapper;

        public BankProviderType ProviderType => BankProviderType.Mellat;

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
                    requestAmount = cmd.RequestAmount.ToString(),
                    approvalCode = cmd.ApprovalCode,
                    cbTrackingCode = cmd.CbTrackingCode,
                    configType = cmd.ConfigType,
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
            var mellatReq = _mapper.Map<MellatDepositReq>(cmd);

            var response = await client.RequestDepositAsync(mellatReq, ct);

            var result = _mapper.Map<DepositRequestResultDto>(response);

            return result;
        }

        public async Task<GetCustomerBillingResultDto> GetCustomerBillingAsync(GetCustomerBillingCommand cmd, CancellationToken ct)
        {
            var mellatReq = _mapper.Map<MellatCustomerBillingReq>(cmd);

            var response = await client.GetCustomerBillingAsync(mellatReq, ct);

            var result = _mapper.Map<GetCustomerBillingResultDto>(response);

            return result;
        }

        public async Task<GetCustomerCreditBalanceResultDto> GetCustomerCreditBalanceAsync(GetCustomerCreditBalanceCommand cmd, CancellationToken ct)
        {
            var mellatReq = _mapper.Map<MellatCustomerCreditBalanceReq>(cmd);

            var response = await client.GetCustomerCreditBalanceAsync(mellatReq, ct);

            var result = _mapper.Map<GetCustomerCreditBalanceResultDto>(response);

            return result;
        }

        public async Task<CustomerInquiryStatusResultDto> GetCustomerInquiryStatusAsync(string requestId, CancellationToken ct)
        {
            var response = await _client.GetInquiryResultAsync(requestId, ct);
            //var result = _mapper.Map<CustomerInquiryStatusResultDto>(response);
            var result = new CustomerInquiryStatusResultDto
            {
                RequestId = requestId,
                Allowed = response.Result.allowed,
                Gender = response.Result.Gender,
                MaxApprovedAmount = response.Result.maxApprovedAmount,
                RequestExpireDate = response.Result.requestExpireDate,
                PostalCodeStatus = response.Result.postalCode,//از سمت بانک بر نمیگرده 
                StatusList = response.Result.statusList?
                             .Select(s => new CustomerInquiryStatusResultDto.StatusItemDto(
                                 s.responseCode,
                                 s.responseStatus
                             ))
                             .ToList()
            };
            return result;
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
                ContractNumber =Convert.ToDecimal( response.contractNumber),
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
                ContractNumber =Convert.ToDecimal( response.contractNumber),
                Message = response.message,
                MessageCode = messageCode
            };
            //var result = _mapper.Map<GetContractFileResultDto>(response);

            return result;
        }

        public async Task<GetPayResponseResultDto> GetPayResponseAsync(string payRequestId, CancellationToken ct)
        {
            var response = await _client.GetPayResponseAsync(new MellatPayReq { PayRequestId = payRequestId }, ct);
            var result = new GetPayResponseResultDto
            {
                PayContractInfo=new GetPayResponseResultDto.PayContractInfoDto
                {
                    CbTrackingCode=response.payContractInfo.cbTrackingCode,
                    ContractDate=response.payContractInfo.contractDate,
                    ContractFile=response.payContractInfo.contractFile,
                    ContractNo=response.payContractInfo.contractNo,
                    InstallmentCount=response.payContractInfo.installmentCount,
                    LoanAmount=response.payContractInfo.loanAmount,
                    NationalCode=response.payContractInfo.nationalCode,
                    SumCost=response.payContractInfo.sumCost,
                    TraceCode=response.payContractInfo.traceCode
                },
                PayRequestStatus = new GetPayResponseResultDto.PayRequestStatusDto
                {
                    ResponseCode= (PayResponseCode)response.payRequestStatus.responseCode
                    
                    
                }
            };
            return result;
        }

        public async Task<ReturnTransferReportResultDto> GetReturnTransferReportAsync(ReturnTransferReportCommand cmd, CancellationToken ct)
        {
            var mellatReq = new MellatReturnTransferReportReq
            {
                fromId=cmd.FromId,
                returnDate=cmd.ReturnDate
            };

            var response = await client.GetReturnTransferReportAsync(mellatReq, ct);

            var result = new ReturnTransferReportResultDto
            {
                FromId = response.fromId,
                Message = response.message,
                ReturnedTransfers = response.returnedTransfers.Select(x => new ReturnTransferReportResultDto.ReturnedTransferDto
                {
                    ApprovalId = x.approvalId,
                    DestBankCode = x.destBankCode,
                    DestIban=x.sourceIban,
                    DestName=x.destName,
                    NoSendDate=x.noSendDate,
                    PayAmount=x.payAmount,
                    ReasonCode=x.reasonCode,
                    RegisterCode=x.registerCode,
                    ReturnDate=x.returnDate,
                    ReturnReasonCode=x.returnReasonCode,
                    ReturnReasonDesc=x.returnReasonDesc,
                    ReturnTime=x.returnTime,
                    RowId=x.rowId,
                    SendDate=x.sendDate,
                    SendTime=x.sendTime,
                    SourceIban=x.sourceIban,
                    TrackingNo=x.trackingNO,
                    TransferDate=x.transferDate,
                    TransferStatus= (ReturnTransferReportResultDto.TransferStatus)x.transferStatus

                }).ToList(),
            };

            return result;
        }
        public async Task<GetCustomerPurchaseDetailsResultDto> GetCustomerPurchaseDetailsAsync(GetCustomerPurchaseDetailsCommand cmd, CancellationToken ct)
        {
            var mellatReq = _mapper.Map<MellatCustomerPurchaseDetailsReq>(cmd);

            var response = await client.CustomerPurchaseDetailsAsync(mellatReq, ct);

            var result = _mapper.Map<GetCustomerPurchaseDetailsResultDto>(response);

            return result;
        }

        public async Task<GetInstallmentsResultDto> GetInstallmentsAsync(string nationalCode, decimal contractNumber, CancellationToken ct)
        {
            var response = await _client.GetInstallmentsAsync(new MellatInstallmentsReq { ContractNumber = contractNumber, NationalCode = nationalCode }, ct);
            var result = _mapper.Map<GetInstallmentsResultDto>(response);
            return result;
        }


        public async Task<TransferRegisterResultDto> RegisterTransferAsync(TransferRegisterCommand cmd, CancellationToken ct)
        {
            var mellatReq = _mapper.Map<MellatTransferRegisterReq>(cmd);

            var response = await client.RegisterTransferAsync(mellatReq, ct);

            var result = _mapper.Map<TransferRegisterResultDto>(response);

            return result;
        }

        public async Task<RepaymentRequestResultDto> RepaymentRequestAsync(RepaymentRequestCommand cmd, CancellationToken ct)
        {
            var mellatReq = _mapper.Map<MellatRepaymentReq>(cmd);

            var response = await client.RequestRepaymentAsync(mellatReq, ct);

            var result = _mapper.Map<RepaymentRequestResultDto>(response);

            return result;
        }

        public async Task<OtpRequestResultDto> RequestOtpAsync(OtpRequestCommand cmd, CancellationToken ct)
        {
            var mellatReq = _mapper.Map<MellatOtpReq>(cmd);

            var response = await client.RequestOtpAsync(mellatReq, ct);

            var result = _mapper.Map<OtpRequestResultDto>(response);

            return result;
        }

        public async Task<SubmitPayRequestResultDto> SubmitPayRequestAsync(SubmitPayRequestCommand cmd, CancellationToken ct)
        {
            var mellatReq = _mapper.Map<MellatSubmitPayRequestReq>(cmd);

            var response = await client.SubmitPayRequestAsync(mellatReq, ct);

            var result = _mapper.Map<SubmitPayRequestResultDto>(response);

            return result;
        }

        public async Task<TransferInquiryResultDto> TransferInquiryAsync(TransferInquiryQuery q, CancellationToken ct)
        {
            var response = await _client.GetTransferInquiryAsync(new MellatTransferInquiryReq { RegisterCode = q.RegisterCode }, ct);
            var result = _mapper.Map<TransferInquiryResultDto>(response);
            return result;
        }
    }
}

