using Common;
using LoanGateway.Auth.Application.UseCase.Command.MobileOwner;
using LoanGateway.Auth.Application.UseCase.Command.RequestOtp;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LoanGateway.Auth.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IMediator mediator, ILogger<AuthController> logger) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly ILogger<AuthController> _logger = logger;

        [HttpPost("createOTP")]
        public async Task<IActionResult> CreateOTP(RequestOtpCommandDto cmd, CancellationToken ct)
        {
            var result = await _mediator.Send(cmd, ct);

            return ToHttp(result);
        }


        [HttpGet("verifyMobileOwner")]
        public async Task<IActionResult> VerifyMobileOwner([FromBody] VerifyMobileOwnerCommand cmd,  CancellationToken ct)
        {
            var result = await _mediator.Send(cmd, ct);

            return ToHttp(result);
        }

        //[HttpGet("loginUser")]
        //public async Task<IActionResult> LoginUser(CancellationToken ct)
        //{
        //    var result = await _mediator.Send(new(), ct);

        //    return ToHttp(result);
        //}

        //[HttpGet("getToken")]
        //public async Task<IActionResult> GetToken(CancellationToken ct)
        //{
        //    var result = await _mediator.Send(new(), ct);

        //    return ToHttp(result);
        //}


        //[HttpGet("refreshToken")]
        //public async Task<IActionResult> RefreshToken(CancellationToken ct)
        //{
        //    var result = await _mediator.Send(new(), ct);

        //    return ToHttp(result);
        //}






        // -------------------- Helper: Result → IActionResult --------------------
        private IActionResult ToHttp<T>(Result<T> result)
        {
            if (result.IsSuccess) return Ok(result.Value);

            var statusCode = result.Error!.Code == 404
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;


            return StatusCode(statusCode, new
            {
                error = result.Error.Message,
                code = result.Error.Code,
                details = result.Error.Details
            });
        }
    }


























}


