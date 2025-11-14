using Common;
using LoanService.Application.UseCase.Command.CustomerInquiry;
using LoanService.Application.UseCase.Command.DepositRequest;
using LoanService.Application.UseCase.Command.GetCollateralContractFile;
using LoanService.Application.UseCase.Command.GetContractFile;
using LoanService.Application.UseCase.Command.GetCustomerBilling;
using LoanService.Application.UseCase.Command.GetCustomerPurchaseDetails;
using LoanService.Application.UseCase.Command.OtpRequest;
using LoanService.Application.UseCase.Command.RepaymentReques;
using LoanService.Application.UseCase.Command.StartLoanRequestDto;
using LoanService.Application.UseCase.Command.SubmitPayRequest;
using LoanService.Application.UseCase.Command.TransferRegister;
using LoanService.Application.UseCase.Query.GetInstallments;
using LoanService.Domain.Entities;
using LoanService.Domain.Enum;
using LoanService.Domain.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace LoanGateway.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoanController : ControllerBase
    {
        private readonly LoanRequestOrchestrator _orchestrator;
        private readonly ILoanRequestRepository _repo;

        public LoanController(LoanRequestOrchestrator orchestrator, ILoanRequestRepository repo)
        {
            _orchestrator = orchestrator;
            _repo = repo;
        }

        /// <summary>
        /// ثبت استعلام
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("start")]
        public async Task<IActionResult> StartInquiry([FromBody] CustomerInquiryCommand cmd, CancellationToken ct)
        {
            var entity = LoanRequest.Create(cmd.NationalCode,cmd.BirthDate,cmd.PostalCode,cmd.MobileNo,cmd.ProviderType,cmd.ApprovalCode,false);
            await _repo.InsertAsync(entity, ct);

            var result = await _orchestrator.StartInquiryAsync(cmd, entity, ct);
            return ToHttp(result);
        }

        /// <summary>
        ///  دریافت نتیجه استعلام
        /// </summary>
        /// <param name="loanId"></param>
        /// <param name="ProviderType"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("{loanId:guid}/inquiry/result")]
        public async Task<IActionResult> InquiryResult(Guid loanId, BankProviderType ProviderType, CancellationToken ct)
        {
            var result = await _orchestrator.GetInquiryResultAsync(loanId, ProviderType, ct);
            return ToHttp(result);
        }
        /// <summary>
        /// فایل قرارداد بدون وثیقه
        /// </summary>
        /// <param name="loanId"></param>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("{loanId:guid}/contracts/no-collateral")]
        public async Task<IActionResult> ContractFileNoCollateral(Guid loanId, [FromBody] GetContractFileCommand cmd, CancellationToken ct)
        {
            var result = await _orchestrator.GetContractFileNoCollateralAsync(loanId, cmd, ct);
            return ToHttp(result);
        }

        /// <summary>
        /// فقط دریافت اقساط از بانک + ذخیره
        /// </summary>
        /// <param name="loanId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>


        [HttpPost("Installments")]
        public async Task<IActionResult> GetInstallments(Guid loanId, CancellationToken ct)
        {
            var result = await _orchestrator.GetInstallmentsAsync(loanId, ct);
            return ToHttp(result);
        }

        /// <summary>
        /// فایل قرارداد با وثیقه
        /// </summary>
        /// <param name="loanId"></param>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("{loanId:guid}/contracts/collateral")]
        public async Task<IActionResult> ContractFileCollateral(Guid loanId, [FromBody] GetCollateralContractFileCommand cmd, CancellationToken ct)
        {
            var result = await _orchestrator.GetContractFileWithCollateralAsync(loanId, cmd, ct);
            return ToHttp(result);
        }

        /// <summary>
        /// ثبت درخواست اعطا 
        /// </summary>
        /// <param name="loanId"></param>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("{loanId:guid}/pay-request")]
        public async Task<IActionResult> PayRequest(Guid loanId, [FromBody] SubmitPayRequestCommand cmd, CancellationToken ct)
        {
            var result = await _orchestrator.SubmitPayRequestAsync(loanId, cmd, ct);
            return ToHttp(result);
        }
        /// <summary>
        /// پاسخ اعطا
        /// </summary>
        /// <param name="loanId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("{loanId:guid}/pay-response")]
        public async Task<IActionResult> PayResponse(Guid loanId, CancellationToken ct)
        {
            var result = await _orchestrator.GetPayResponseAsync(loanId, ct);
            return ToHttp(result);
        }

        /// <summary>
        ///  ارسال OTP
        /// </summary>
        /// <param name="loanId"></param>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("{loanId:guid}/otp-request")]
        public async Task<IActionResult> OtpRequest(Guid loanId, [FromBody] OtpRequestCommand cmd, CancellationToken ct)
        {
            var result = await _orchestrator.SendOtpAsync(loanId, cmd, ct);
            return ToHttp(result);
        }

        /// <summary>
        /// ثبت حواله
        /// </summary>
        /// <param name="loanId"></param>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("{loanId:guid}/transfer/register")]
        public async Task<IActionResult> TransferRegister(Guid loanId, [FromBody] TransferRegisterCommand cmd, CancellationToken ct)
        {
            var result = await _orchestrator.TransferRegisterAsync(loanId, cmd, ct);
            return ToHttp(result);
        }

        /// <summary>
        /// استعلام حواله
        /// </summary>
        /// <param name="loanId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpGet("{loanId:guid}/transfer/inquiry")]
        public async Task<IActionResult> TransferInquiry(Guid loanId, CancellationToken ct)
        {
            var result = await _orchestrator.TransferInquiryAsync(loanId, ct);
            return ToHttp(result);
        }

        /// <summary>
        /// مانده اعتبار قرارداد
        /// </summary>
        /// <param name="loanId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>

        [HttpPost("creditBalance")]
        public async Task<IActionResult> CreditBalance(Guid loanId, CancellationToken ct)
        {
            var result = await _orchestrator.GetCreditBalanceAsync(loanId, ct);
            return ToHttp(result);
        }
        /// <summary>
        ///  درخواست واریز وجه
        /// </summary>
        /// <param name="loanId"></param>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("deposit")]
        public async Task<IActionResult> DepositRequest(Guid loanId, DepositRequestCommand cmd, CancellationToken ct)
        {
            var result = await _orchestrator.DepositRequestAsync(loanId,cmd, ct);
            return ToHttp(result);
        }

        /// <summary>
        /// بازپرداخت بدهی
        /// </summary>
        /// <param name="loanId"></param>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("reyPayment")]
        public async Task<IActionResult> RepaymentRequest(Guid loanId, RepaymentRequestCommand cmd, CancellationToken ct)
        {
            var result = await _orchestrator.RepaymentRequestAsync(loanId, cmd, ct);
            return ToHttp(result);
        }


        /// <summary>
        ///  دریافت فهرست ریزخریدها 
        /// </summary>
        /// <param name="loanId"></param>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("purchases")]
        public async Task<IActionResult> GetPurchasesAsync(Guid loanId, GetCustomerPurchaseDetailsCommand cmd, CancellationToken ct)
        {
            var result = await _orchestrator.GetPurchasesAsync(loanId, cmd, ct);
            return ToHttp(result);
        }

        /// <summary>
        ///  صورتحساب
        /// </summary>
        /// <param name="loanId"></param>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("billing")]
        public async Task<IActionResult> GetBillingAsync(Guid loanId, GetCustomerBillingCommand cmd, CancellationToken ct)
        {
            var result = await _orchestrator.GetBillingAsync(loanId, cmd, ct);
            return ToHttp(result);
        }

        // -------------------- Helper: Result → IActionResult --------------------
        private IActionResult ToHttp<T>(Result<T> result)
        {
            if (result.IsSuccess) return Ok(result.Value);
            return BadRequest(new { error = result.Error!.Message, code = result.Error.Code });
        }
    }


























}


