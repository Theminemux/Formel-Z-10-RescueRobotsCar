using Microsoft.AspNetCore.Mvc;
using RescueRobotsCar.API.Models.Responses;
using RescueRobotsCar.Driver.LineSensors;
using RescueRobotsCar.Driver.RFID;
using RescueRobotsCar.Driving.Sensors;
using RescueRobotsCar.Services;

namespace RescueRobotsCar.API.Controller
{
    [ApiController]
    [Route("sensors")]
    public class SensorsController : ControllerBase
    {
        private readonly RFIDRC522Driver _rfidDriver;
        private readonly Compass _compass;
        private readonly LineSensor _lineSensor;
        private readonly StatusProvider _statusProvider;
        private readonly CollectedObjectsManager _objectManager;

        public SensorsController(RFIDRC522Driver rfiddriver, Compass compass, LineSensor lineSensor, StatusProvider statusProvider, CollectedObjectsManager objectManager)
        {
            _rfidDriver = rfiddriver;
            _compass = compass;
            _lineSensor = lineSensor;
            _statusProvider = statusProvider;
            _objectManager = objectManager;
        }

        [HttpPost("rfidupdate")]
        public async Task<IActionResult> PostRFIDUpdate(Dictionary<string, string> body)
        {
            var status = await _statusProvider.GetStatusAsync();
            if (status.Status != (int)StatusContainer.EStatus.Driving)
                return Ok();

            if (!body.ContainsKey("rfid_reader") || !body.ContainsKey("data"))
            {
                Console.WriteLine("Invalid RFID update request received. Missing 'rfid_reader' or 'data' in the request body.");
                return BadRequest();
            }

            if (body["data"].StartsWith("OBJ"))
            {
                // Object collected
                _objectManager.AddCollectedObject(body["data"]);
            }
            else
            {
                // Track coordinate scanned
            }

            _rfidDriver.UpdateCardData(body["data"]);
            return Ok();
        }

        [HttpGet("getlinesensorvalues")]
        public IActionResult GetLineSensorValues()
        {
            LineSensorValuesResponse response = new LineSensorValuesResponse
            {
                Left = _lineSensor.SensorValuesLeftRight[0],
                LeftCenter = _lineSensor.SensorValuesLeftRight[1],
                Center = _lineSensor.SensorValuesLeftRight[2],
                RightCenter = _lineSensor.SensorValuesLeftRight[3],
                Right = _lineSensor.SensorValuesLeftRight[4],
                CalculatedMidpoint = LineSensorsMidpoint.CalculateMidpoint(_lineSensor.SensorValuesLeftRight)
            };
            return Ok(response);
        }

        [HttpGet("getcompassvalue")]
        public IActionResult GetCompass()
        {
            return Ok(_compass.CurrentAngle);
        }

        [HttpGet("resetcompass")]
        public IActionResult ResetCompass()
        {
            _compass.ResetAngle();
            return Ok();
        }
    }
}
