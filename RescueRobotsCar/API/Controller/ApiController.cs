using Microsoft.AspNetCore.Mvc;
using RescueRobotsCar.Driver.LineSensors;
using RescueRobotsCar.Services;
using RescueRobotsCar.Driving.Maps.MapObjects;

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
        private readonly StatusSetter _statusSetter;
        private readonly LineFollower _lineFollower;

        public ApiController(
            LineSensor lineSensor,
            SystemStateService systemStateService,
            MapObjectsProvider mapObjectsProvider,
            StatusProvider statusProvider,
            StatusSetter statusSetter,
            LineFollower lineFollower)
        {
            _lineSensor = lineSensor;
            _systemState = systemStateService;
            _mapObjectsProvider = mapObjectsProvider;
            _statusProvider = statusProvider;
            _lineFollower = lineFollower;
            _statusSetter = statusSetter;
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
        public async Task<IActionResult> Start()
        {
            Console.WriteLine("Start command received.");
            await _statusSetter.SetStatusAsync(StatusSetter.EStatus.Driving);
            return Ok();
        }

        [HttpGet("pause")]
        public async Task<IActionResult> Pause()
        {
            Console.WriteLine("Pause command received.");
            await _statusSetter.SetStatusAsync(StatusSetter.EStatus.Pause);
            return Ok();
        }

        [HttpGet("resume")]
        public async Task<IActionResult> Resume()
        {
            if ((await _statusSetter.GetStatusAsync()) != (int)StatusSetter.EStatus.Pause)
            {
                return BadRequest("Cannot resume because the system is not paused.");
            }
            Console.WriteLine("Resume command received.");
            await _statusSetter.SetStatusAsync(StatusSetter.EStatus.Driving);
            return Ok();
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
            StatusContainer status = await _statusProvider.GetStatusAsync();
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

        [HttpGet("debugLineFollowerStart")]
        public async Task<IActionResult> DebugLineFollowerStart()
        {
            Console.WriteLine("Debug Line Follower Start command received.");
            _ = _lineFollower.Start();
            return Ok();
        }

        [HttpGet("debugLineFollowerStop")]
        public async Task<IActionResult> DebugLineFollowerStop()
        {
            Console.WriteLine("Debug Line Follower Stop command received.");
            await _lineFollower.Stop();
            return Ok();
        }

        [HttpGet("debugLineFollowerLiveData")]
        public IActionResult DebugLineFollowerLiveData()
        {
            var debugData = _lineFollower.GetDebugData();
            return Ok(debugData);
        }
    }
}
