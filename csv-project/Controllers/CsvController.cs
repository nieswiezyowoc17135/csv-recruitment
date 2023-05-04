using csv_project.Features.Query;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace csv_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CsvController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CsvController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet]
        public async Task<IActionResult> GetOrders(string? order, DateTime? fromWhen, DateTime? toWhen, [FromQuery(Name = "clientCodes")] List<string>? clientCodes)
        {
            var result = await _mediator.Send(new CsvReadQuery(order ?? null, fromWhen ?? null, toWhen ?? null, clientCodes ?? null));
            return Ok(result);
        }
    }
}
