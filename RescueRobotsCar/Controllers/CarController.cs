using Microsoft.AspNetCore.Mvc;
using RescueRobotsCar.Models.Navigation;
using RescueRobotsCar.Services;
using RescueRobotsCar.Driver.Motor;
using Microsoft.AspNetCore.Http.HttpResults;
using RescueRobotsCar.Driver.RFID;

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

    [ApiController]
    [Route("sensors")]
    public class SensorsController : ControllerBase
    {
        private readonly RFIDRC522Driver _rfidDriver;

        public SensorsController(RFIDRC522Driver rfiddriver)
        {
            _rfidDriver = rfiddriver;
        }

        [HttpPost("rfidupdate")]
        public IActionResult PostRFIDUpdate(Dictionary<string, string> body)
        {
            if (!body.ContainsKey("rfid_reader") || !body.ContainsKey("data"))
            {
                Console.WriteLine("Invalid RFID update request received. Missing 'rfid_reader' or 'data' in the request body.");
                return BadRequest();
            }
            _rfidDriver.UpdateCardData(body["data"]);
            return Ok();
        }
    }
}
