using Microsoft.AspNetCore.Mvc;
using System;

namespace ClickersAPI.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RootController : ControllerBase
    {
        [HttpGet("timetest")]
        public IActionResult GetCurrentTime()
        {
            const string message = "UTC date and time";
            return Ok(new {message, DateTime.UtcNow});
        }
    }
}
