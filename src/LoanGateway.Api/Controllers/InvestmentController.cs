using Common;
using LoanService.Application.Contracts;
using LoanService.Application.UseCase.Investment.Command.BuyPlan;
using LoanService.Application.UseCase.Investment.Command.CompleteBuy;
using LoanService.Application.UseCase.Investment.Command.CompleteGiftCard;
using LoanService.Application.UseCase.Investment.Command.CompletePayment;
using LoanService.Application.UseCase.Investment.Command.GiftCard;
using LoanService.Application.UseCase.Investment.Command.PaymentGateway;
using LoanService.Application.UseCase.Investment.Command.User;
using LoanService.Application.UseCase.Investment.Query.GetInvestmentDetailsPlans;
using LoanService.Application.UseCase.Investment.Query.GetInvestmentPlans;
using LoanService.Application.UseCase.Investment.Query.GiftCard;
using LoanService.Application.UseCase.Investment.Query.OrderBuy;
using LoanService.Application.UseCase.Investment.Query.PlanBuyInfo;
using LoanService.Domain.Enum.Investment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanGateway.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvestmentController(IMediator mediator, ILogger<InvestmentController> logger, IUserContext userContext) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly ILogger<InvestmentController> _logger = logger;
        private readonly IUserContext _userContext = userContext;


        [Authorize]
        [HttpGet("get-investment-plan")]
        public async Task<IActionResult> GetInvestmentPlan(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetInvestmentPlansQuery(), ct);

            return ToHttp(result);
        }

        [HttpGet("{planType}/details")]

        public async Task<IActionResult> GetPlanDetails([FromRoute] InvestmentPlanType planType, [FromQuery] InvestmentChartRange range, [FromQuery]InvestmentBoxStatus boxStatus, CancellationToken ct = default)
        {
            _logger.LogInformation("GetPlanDetails called. planType={PlanType}, range={Range}", planType, range);

            var result = await _mediator.Send(new GetInvestmentPlanDetailsQuery(planType, range, boxStatus), ct);
            return ToHttp(result);
        }

        [Authorize]
        [HttpGet("{planType}/get-plan-buy-info")]
        public async Task<IActionResult> GetPlanBuyInfo([FromRoute] InvestmentPlanType planType, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new PlanBuyInfoQueryDto(planType), ct);
            return ToHttp(result);
        }



        [Authorize]
        [HttpGet("getOrder-buy-ByOrder")]
        public async Task<IActionResult> GetOrderBuyByOrder(Guid id, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new GetOrderStatusQuery { Id=id}, ct);
            return ToHttp(result);
        }

        [Authorize]
        [HttpPost("submit-buy")]
        public async Task<IActionResult> SubmitBuy([FromBody] BuyPlanCommand command, CancellationToken ct)
        {

            var result = await _mediator.Send(command, ct);
            return ToHttp(result);
        }


        [Authorize]
        [HttpPost("payment")]
        public async Task<IActionResult> Payment([FromBody] StartSadadPaymentCommand cmd, CancellationToken ct)
        {

            var result = await _mediator.Send(cmd, ct);
            return ToHttp(result);
        }

        [Authorize]
        [HttpPost("callback")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public async Task<IActionResult> Callback([FromForm] SadadCallbackDto form, CancellationToken ct)
        {
            var dto = new SadadCallbackDto(form.OrderId, form.Token, form.ResCode);
            var result = await mediator.Send(new CompleteSadadPaymentCommand(dto), ct);

            return Ok(result);
        }


        [HttpPost("receive")]
        public async Task<IActionResult> ReceiveZipCode([FromBody] ReceiveZipCodeCommand cmd,CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(cmd.PostalCode))
            {
                return BadRequest("کدپستی وارد نشده است.");
            }
            var result = await _mediator.Send(cmd, ct);
            return ToHttp(result);
        }

  
        [Authorize]
        [HttpGet("gift-card/can-see")]
        public async Task<IActionResult> CanSeeGiftCard(CancellationToken ct)
        {
            var query = new CanSeeGiftCardQuery
            {
                UserId = _userContext.UserId
            };
            var result = await _mediator.Send(query, ct);
            return ToHttp(result);
        }


        [Authorize]
        [HttpPost("gift-card/receive")]
        public async Task<IActionResult> ReceiveGiftCard(CancellationToken ct)
        {
            var command = new ReceiveGiftCardCommand
            {
                UserId = _userContext.UserId
            };
            var result = await _mediator.Send(command, ct);
            return ToHttp(result);
        }

        [Authorize]
        [HttpPost("complete-buy")]
        public async Task<IActionResult> CompleteBuy([FromBody] CompleteBuyCommand command, CancellationToken ct)
        {
            command.UserId = _userContext.UserId; 
            var result = await _mediator.Send(command, ct);
            return ToHttp(result);
        }



        // -------------------- Helper: Result → IActionResult --------------------
        private IActionResult ToHttp<T>(Result<T> result)
        {
            if (result.IsSuccess) return Ok(result.Value);

            // برای خطاهای کسب‌وکار از سرویس خارجی (مثل کاریزما) کد 200 استفاده می‌شود
            if (result.Error!.Code == 200)
            {
                return Ok(new
                {
                    error = result.Error.Message,
                    code = result.Error.Code,
                    details = result.Error.Details
                });
            }

            var statusCode = result.Error.Code == 404
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


