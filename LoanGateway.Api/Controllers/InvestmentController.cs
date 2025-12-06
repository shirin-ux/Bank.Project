using Common;
using LoanService.Application.UseCase.Investment.Command.BuyPlanCommand;
using LoanService.Application.UseCase.Investment.Query.GetInvestmentDetailsPlans;
using LoanService.Application.UseCase.Investment.Query.GetInvestmentPlans;
using LoanService.Application.UseCase.Investment.Query.PlanBuyInfo;
using LoanService.Domain.Enum.Investment;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LoanGateway.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvestmentController(IMediator mediator, ILogger<InvestmentController> logger) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly ILogger<InvestmentController> _logger = logger;

        [HttpGet("getInvestmentPlan")]
        public async Task<IActionResult> GetInvestmentPlan(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetInvestmentPlansQuery(), ct);

            return ToHttp(result);
        }


        /// <summary>
        /// جزئیات طرح سرمایه‌گذاری 
        /// </summary>
        [HttpGet("{planType}/details")]
      
        public async Task<IActionResult> GetPlanDetails([FromRoute] InvestmentPlanType planType, [FromQuery]InvestmentChartRange range ,CancellationToken ct = default)
        {
            _logger.LogInformation("GetPlanDetails called. planType={PlanType}, range={Range}", planType, range);

            var result = await _mediator.Send(new GetInvestmentPlanDetailsQuery(planType, range), ct);
           return ToHttp(result);
        }
        [HttpGet("{planType}/getPlanBuyInfo")]
        public async Task<IActionResult> GetPlanBuyInfo([FromRoute] InvestmentPlanType planType, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new PlanBuyInfoQueryDto(planType), ct);
            return ToHttp(result);
        }
        [HttpPost("plans/buy")]
        public async Task<IActionResult> SubmitBuy([FromBody] BuyPlanCommand command, CancellationToken ct)
        {

            var result = await _mediator.Send(command, ct);
            return ToHttp(result);
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


