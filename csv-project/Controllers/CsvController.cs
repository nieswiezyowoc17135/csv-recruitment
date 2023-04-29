using Microsoft.AspNetCore.Mvc;

namespace csv_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CsvController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetOrders(string? order, DateTime? fromWhen, DateTime? toWhen, [FromQuery(Name = "clientCodes")] List<string>? clientCodes)
        {
            if (order != null && fromWhen==null && toWhen==null && clientCodes!.Count==0)
            {
                var temp = "siemka";
            } else if(fromWhen != null && toWhen!=null && order==null && clientCodes!.Count == 0)
            {
                var temp = "siemka";
            } else if(clientCodes!.Count >= 1 && order==null && fromWhen == null && toWhen == null)
            {
                var temp = "siemka";
            } else if (order != null && fromWhen != null && toWhen != null && clientCodes!.Count > 1)
            {
                var temp = "siemka";
            }

            return Ok();
        }
    }
}
