using Common;
using LoanService.Application.UseCase.Investment.Query.GetInvestmentPlans;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LoanGateway.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvestmentController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;


        [HttpGet("getInvestmentPlan")]
        public async Task<IActionResult> GetInvestmentPlan(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetInvestmentPlansQuery(), ct);

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


