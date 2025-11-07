using Common;
using LoanService.Application.UseCase.Command.CustomerInquiry;
using LoanService.Application.UseCase.Command.DepositRequest;
using LoanService.Application.UseCase.Command.GetCollateralContractFile;
using LoanService.Application.UseCase.Command.GetContractFile;
using LoanService.Application.UseCase.Command.OtpRequest;
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

        // -------------------- 1) ثبت استعلام --------------------
        [HttpPost("start")]
        public async Task<IActionResult> StartInquiry([FromBody] CustomerInquiryCommand cmd, CancellationToken ct)
        {
            var entity = LoanRequest.Create(cmd.NationalCode,cmd.BirthDate,cmd.PostalCode,cmd.MobileNo,cmd.ProviderType,cmd.ApprovalCode,false);
            await _repo.InsertAsync(entity, ct);

            var result = await _orchestrator.StartInquiryAsync(entity.Id, cmd, entity, ct);
            return ToHttp(result);
        }

        // -------------------- 2) دریافت نتیجه استعلام --------------------
        [HttpGet("{loanId:guid}/inquiry/result")]
        public async Task<IActionResult> InquiryResult(Guid loanId, BankProviderType ProviderType, CancellationToken ct)
        {
            var result = await _orchestrator.GetInquiryResultAsync(loanId, ProviderType, ct);
            return ToHttp(result);
        }

        // -------------------- 3) فایل قرارداد بدون وثیقه --------------------
        [HttpPost("{loanId:guid}/contracts/no-collateral")]
        public async Task<IActionResult> ContractFileNoCollateral(Guid loanId, [FromBody] GetContractFileCommand cmd, CancellationToken ct)
        {
            var result = await _orchestrator.GetContractFileNoCollateralAsync(loanId, cmd, ct);
            return ToHttp(result);
        }

        // -------------------- 10) فقط دریافت اقساط از بانک + ذخیره --------------------


        [HttpPost("Installments")]
        public async Task<IActionResult> GetInstallments(Guid loanId, CancellationToken ct)
        {
            var result = await _orchestrator.GetInstallmentsAsync(loanId, ct);
            return ToHttp(result);
        }

        // -------------------- 4) فایل قرارداد با وثیقه --------------------
        [HttpPost("{loanId:guid}/contracts/collateral")]
        public async Task<IActionResult> ContractFileCollateral(Guid loanId, [FromBody] GetCollateralContractFileCommand cmd, CancellationToken ct)
        {
            var result = await _orchestrator.GetContractFileWithCollateralAsync(loanId, cmd, ct);
            return ToHttp(result);
        }


        // -------------------- 5) ثبت درخواست اعطا --------------------
        [HttpPost("{loanId:guid}/pay-request")]
        public async Task<IActionResult> PayRequest(Guid loanId, [FromBody] SubmitPayRequestCommand cmd, CancellationToken ct)
        {
            var result = await _orchestrator.SubmitPayRequestAsync(loanId, cmd, ct);
            return ToHttp(result);
        }

        // -------------------- 6) پاسخ اعطا --------------------
        [HttpGet("{loanId:guid}/pay-response")]
        public async Task<IActionResult> PayResponse(Guid loanId, CancellationToken ct)
        {
            var result = await _orchestrator.GetPayResponseAsync(loanId, ct);
            return ToHttp(result);
        }

        // -------------------- 7) ارسال OTP --------------------
        [HttpPost("{loanId:guid}/otp-request")]
        public async Task<IActionResult> OtpRequest(Guid loanId, [FromBody] OtpRequestCommand cmd, CancellationToken ct)
        {
            var result = await _orchestrator.SendOtpAsync(loanId, cmd, ct);
            return ToHttp(result);
        }

        // -------------------- 8) ثبت حواله --------------------
        [HttpPost("{loanId:guid}/transfer/register")]
        public async Task<IActionResult> TransferRegister(Guid loanId, [FromBody] TransferRegisterCommand cmd, CancellationToken ct)
        {
            var result = await _orchestrator.TransferRegisterAsync(loanId, cmd, ct);
            return ToHttp(result);
        }

        // -------------------- 9) استعلام حواله --------------------
        [HttpGet("{loanId:guid}/transfer/inquiry")]
        public async Task<IActionResult> TransferInquiry(Guid loanId, CancellationToken ct)
        {
            var result = await _orchestrator.TransferInquiryAsync(loanId, ct);
            return ToHttp(result);
        }

        // ---------------- 11) مانده اعتبار قرارداد --------------

        [HttpPost("creditBalanc")]
        public async Task<IActionResult> CreditBalance(Guid loanId, CancellationToken ct)
        {
            var result = await _orchestrator.GetCreditBalanceAsync(loanId, ct);
            return ToHttp(result);
        }

        // ---------------- 11)  --------------
        [HttpPost("deposit")]
        public async Task<IActionResult> DepositRequest(Guid loanId, DepositRequestCommand cmd, CancellationToken ct)
        {
            var result = await _orchestrator.DepositRequestAsync(loanId,cmd, ct);
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


