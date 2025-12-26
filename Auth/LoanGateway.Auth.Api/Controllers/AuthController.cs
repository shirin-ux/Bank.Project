using Common;
using LoanGateway.Auth.Application.Adapter.User;
using LoanGateway.Auth.Application.Commons;
using LoanGateway.Auth.Application.UseCase.Command.CompleteProfile;
using LoanGateway.Auth.Application.UseCase.Command.Logout;
using LoanGateway.Auth.Application.UseCase.Command.MobileOwner;
using LoanGateway.Auth.Application.UseCase.Command.RefreshToken;
using LoanGateway.Auth.Application.UseCase.Command.RequestOtp;
using LoanGateway.Auth.Application.UseCase.Command.UpdateUser;
using LoanGateway.Auth.Application.UseCase.Command.UserInfo;
using LoanGateway.Auth.Application.UseCase.Command.VerfiyOtp;
using LoanGateway.Auth.Application.UseCase.Query.CheckUser;
using LoanGateway.Auth.Domain.IRepository;
using LoanGateway.Auth.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LoanGateway.Auth.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IMediator mediator,
        ILogger<AuthController> logger,
        IAuthCookieService authCookieService,
        IUserInfo userService) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly ILogger<AuthController> _logger = logger;
        private readonly IAuthCookieService _authCookieService= authCookieService;
        private readonly IUserInfo _userService= userService;

        [HttpPost("create-otp")]
        public async Task<IActionResult> CreateOTP([FromBody] RequestOtpCommandDto cmd, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("OTP request for phone: {PhoneNumber}, Purpose: {Purpose}",
                cmd.PhoneNumber, cmd.Purpose);

            var result = await _mediator.Send(cmd, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("OTP sent successfully to {PhoneNumber}", cmd.PhoneNumber);
            }
            else
            {
                _logger.LogWarning("OTP request failed for {PhoneNumber}: {Error}",
                    cmd.PhoneNumber, result.Error?.Message);
            }

            return ToHttp(result);
        }
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOTP([FromBody] VerifyOtpCommandDto cmd, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("OTP verification attempt for phone: {PhoneNumber}, Purpose: {Purpose}",
                cmd.MobileNumber, cmd.Purpose);

            var result = await _mediator.Send(cmd, ct);

            if (result.IsSuccess && result.Value != null)
            {
     
                _authCookieService.SetRefreshToken(HttpContext,result.Value.RefreshToken, result.Value.RefreshTokenExpiresAtUtc);


                var responseWithoutRefreshToken = new
                {
                    result.Value.IsValid,
                    result.Value.IsBlocked,
                    result.Value.UserId,
                    result.Value.Purpose,
                    result.Value.IsProfileCompleted,
                    result.Value.IsNewUser,
                    result.Value.AccessToken,
                    result.Value.AccessTokenExpiresAtUtc
                    // RefreshToken removed from response
                };

                _logger.LogInformation("OTP verified successfully for user {UserId}", result.Value.UserId);
                return Ok(responseWithoutRefreshToken);
            }
            else
            {
                _logger.LogWarning("OTP verification failed for {PhoneNumber}: {Error}",
                    cmd.MobileNumber, result.Error?.Message);
                return ToHttp(result);
            }
        }
       
        
        [Authorize]
        [HttpPost("complete-profile")]
        public async Task<IActionResult> CompleteProfile([FromBody] CompleteProfileCommand cmd, CancellationToken ct)
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


        [Authorize]
        [HttpGet("get-user")]
        public async Task<IActionResult> GetUserInfo(CancellationToken ct)
        {
            var result = await _userService.GetUserInfo(ct);

            return ToHttp(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(CancellationToken ct)
        {
            var refreshToken = _authCookieService.GetRefreshToken(HttpContext);
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                _logger.LogWarning("Refresh token cookie is missing or invalid");
                return BadRequest(new
                {
                    error = "رفرش توکن یافت نشد.",
                    code = -1,
                    details = (object?)null
                });
            }
            //var refreshToken = Request.Cookies["RefreshToken"];
            //if (string.IsNullOrWhiteSpace(refreshToken))
            //{
            //    _logger.LogWarning("Refresh token cookie is missing");
            //    return BadRequest(new
            //    {
            //        error = "رفرش توکن یافت نشد.",
            //        code = -1,
            //        details = (object?)null
            //    });
            //}

            // -------------------- خواندن access token از هدر Authorization (اختیاری) --------------------
            var accessToken = Request.Headers["Authorization"]
                .FirstOrDefault()
                ?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Last();

            var cmd = new RefreshTokenCommandDto
            {
                AccessToken = accessToken ?? string.Empty,
                RefreshToken = refreshToken
            };

            if (!TryValidateModel(cmd))
                return BadRequest(ModelState);

            _logger.LogInformation("Refresh token request received (from cookie)");

            var result = await _mediator.Send(cmd, ct);

            if (result.IsSuccess && result.Value != null)
            {

                _authCookieService.SetRefreshToken(
                    HttpContext,
                    result.Value.RefreshToken,
                    result.Value.RefreshTokenExpiresAtUtc);

                var responseWithoutRefreshToken = new
                {
                    result.Value.AccessToken,
                    result.Value.AccessTokenExpiresAtUtc
                    
                };

                _logger.LogInformation("Token refreshed successfully");
                return Ok(responseWithoutRefreshToken);
            }
            else
            {
                _logger.LogWarning("Token refresh failed: {Error}", result.Error?.Message);
                return ToHttp(result);
            }

        }
        [HttpPost("get-user-by-nationalCode")]
     
        public async Task<IActionResult> GetUserByNationalCode([FromBody] UserInfoCommand cmd,CancellationToken ct)
        {
            var result = await _mediator.Send(cmd, ct);
            return ToHttp(result);
        }

     
        [HttpGet("exists/{userId}")]
     
        public async Task<IActionResult> CheckExists(Guid userId)
        {
            var exists = await _mediator.Send(new CheckUserExistsQuery { UserId = userId });
            return Ok(new { UserId = userId, Exists = exists });
        }

 
        [HttpPost("update-postalcode")]
 
        public async Task<IActionResult> UpdatePostalCode([FromBody] UpdatePostalCodeCommand command)
        {
 
            
                var result = await _mediator.Send(command);
                return Ok(result);

  
        }


        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            // خواندن refresh token از کوکی امن
            var refreshToken = Request.Cookies["RefreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var cmd = new LogoutCommandDto(refreshToken);
                await _mediator.Send(cmd, ct);
            }
            _authCookieService.ClearRefreshToken(HttpContext);


            return NoContent();
        }



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


