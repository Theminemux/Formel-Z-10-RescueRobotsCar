using Microsoft.AspNetCore.Mvc;
using RescueRobotsCar.Models.Navigation;
using RescueRobotsCar.Services;
using RescueRobotsCar.Driver.Motor;

namespace RescueRobotsCar.Controllers
{
    [ApiController]
    [Route("Map")]
    public class MapController : ControllerBase
    {
        private Logger _logger;
        private NavigatorService _navigator;

        public MapController(Logger logger, NavigatorService navigator)
        {
            _logger = logger;
            _navigator = navigator;
        }

        [HttpPost("uploadmap")]
        public IActionResult UploadMap([FromBody] TrackMap map)
        {
            _logger.Log("Incoming API MapUpload Request", Logger.Severity.Info);

            _navigator.LoadMap(map);

            return Ok();
        }
    }

    [ApiController]
    [Route("CarControls")]
    public class CarControlsController : ControllerBase
    {
        private Logger _logger;
        private MotorDriver _motor;

        public CarControlsController(Logger logger, MotorDriver motor)
        {
            _logger = logger;
            _motor = motor;
        }

        [HttpGet("test-start")]
        public IActionResult TestStart()
        {
            _logger.Log("Incoming API TestStart Request", Logger.Severity.Info);

            _motor.TestAllMotors();

            return Ok();
        }

        [HttpGet("start")]
        public IActionResult Start()
        {
            _logger.Log("Incoming API Start Request", Logger.Severity.Info);

            return Ok();
        }
        [HttpGet("stop")]
        public IActionResult Stop()
        {
            _logger.Log("Incoming API Stop Request", Logger.Severity.Info);

            return Ok();
        }
    }
}
