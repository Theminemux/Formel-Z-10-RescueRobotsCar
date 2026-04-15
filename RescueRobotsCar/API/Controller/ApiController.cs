using Microsoft.AspNetCore.Mvc;
using RescueRobotsCar.Driver.LineSensors;
using RescueRobotsCar.Services;
using RescueRobotsCar.Driving.Maps.MapObjects;
using RescueRobotsCar.Driving.Maps.Status;

namespace RescueRobotsCar.API.Controller
{
    [ApiController]
    [Route("api")]
    public class ApiController : ControllerBase
    {
        private readonly SystemStateService _systemState;
        private readonly LineSensor _lineSensor;
        private readonly MapObjectsProvider _mapObjectsProvider;
        private readonly StatusProvider _statusProvider;

        public ApiController(
            LineSensor lineSensor, 
            SystemStateService systemStateService,
            MapObjectsProvider mapObjectsProvider,
            StatusProvider statusProvider)
        {
            _lineSensor = lineSensor;
            _systemState = systemStateService;
            _mapObjectsProvider = mapObjectsProvider;
            _statusProvider = statusProvider;
        }

        [HttpGet("checkconnection")]
        public IActionResult CheckConnection()
        {
            return Ok();
        }

        [HttpPost("newjson")]
        public async Task<IActionResult> ImportNewRoute(MapObjectsContainer body)
        {
            await _mapObjectsProvider.UpdateMapObjects(body);
            return Ok();
        }

        [HttpGet("start")]
        public IActionResult Start()
        {
            return NotFound();
        }

        [HttpGet("pause")]
        public IActionResult Stop()
        {
            return NotFound();
        }

        [HttpGet("resume")]
        public IActionResult Resume()
        {
            return NotFound();
        }

        [HttpGet("reset")]
        public IActionResult Reset()
        {
            return NotFound();
        }

        [HttpGet("linesensor")]
        public IActionResult LineSensor()
        {
            return Ok(_lineSensor.SensorValuesLeftRight);
        }

        [HttpGet("status")]
        public async Task<IActionResult> Status()
        {
            StatusContainer status = await _statusProvider.GetStatus();
            return Ok(status);
        }

        [HttpGet("debugStatus")]
        public IActionResult DebugStatus()
        {
            string status = $"LoggedIn: {_systemState.IsLoggedIn}, \n" +
                            $"OrangePiIp: {_systemState.OrangePiIp}, \n" +
                            $"Esp32Connected: {_systemState.IsEsp32Connected}, \n" +
                            $"MotorsInitialized: {_systemState.IsMotorsInitialized}, \n" +
                            $"MapLoaded: {_systemState.IsMapLoaded}, \n" +
                            $"CompassCalibrated: {_systemState.IsCompassCalibrated}";
            return Ok(status);
        }
    }
}
