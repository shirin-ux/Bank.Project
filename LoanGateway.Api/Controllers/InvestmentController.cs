using Common;
using LoanService.Application.UseCase.Investment.Command.BuyPlanCommand;
using LoanService.Application.UseCase.Investment.Command.CompletePayment;
using LoanService.Application.UseCase.Investment.Command.PaymentGateway;
using LoanService.Application.UseCase.Investment.Command.User;
using LoanService.Application.UseCase.Investment.Query.GetInvestmentDetailsPlans;
using LoanService.Application.UseCase.Investment.Query.GetInvestmentPlans;
using LoanService.Application.UseCase.Investment.Query.PlanBuyInfo;
using LoanService.Domain.Enum.Investment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanGateway.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvestmentController(IMediator mediator, ILogger<InvestmentController> logger) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly ILogger<InvestmentController> _logger = logger;
        [Authorize]
        [HttpGet("get-investment-plan")]
        public async Task<IActionResult> GetInvestmentPlan(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetInvestmentPlansQuery(), ct);

            return ToHttp(result);
        }


        /// <summary>
        /// جزئیات طرح سرمایه‌گذاری 
        /// </summary>

        [Authorize]
        [HttpGet("{planType}/details")]

        public async Task<IActionResult> GetPlanDetails([FromRoute] InvestmentPlanType planType, [FromQuery] InvestmentChartRange range, CancellationToken ct = default)
        {
            _logger.LogInformation("GetPlanDetails called. planType={PlanType}, range={Range}", planType, range);

            var result = await _mediator.Send(new GetInvestmentPlanDetailsQuery(planType, range), ct);
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



        [HttpGet("redirect-to-realestate")]
        public IActionResult RedirectToRealEstate()
        {
            var url = "https://amlak.mrud.ir/"; 
            return Redirect(url);
        }

        [HttpPost("receive")]
        public IActionResult ReceiveZipCode([FromBody] ZipCodeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PostalCode))
            {
                return BadRequest("کدپستی وارد نشده است.");
            }
   
            return Ok(new { ReceivedZipCode = request.PostalCode, Message = "کدپستی دریافت شد" });
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


