using Microsoft.AspNetCore.Mvc;

namespace RescueRobotsCar.Controllers
{
    [ApiController]
    [Route("api")]
    public class ApiController : ControllerBase
    {
        [HttpGet("checkconnection")]
        public IActionResult CheckConnection()
        {
            return Ok();
        }
    }
}
