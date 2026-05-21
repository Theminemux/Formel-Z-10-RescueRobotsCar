using Microsoft.AspNetCore.Mvc;
using RescueRobotsCar.API.Models.Responses;
using RescueRobotsCar.Driver.LineSensors;
using RescueRobotsCar.Driver.RFID;
using RescueRobotsCar.Driving;
using RescueRobotsCar.Services;

namespace RescueRobotsCar.API.Controller
{
    [ApiController]
    [Route("sensors")]
    public class SensorsController : ControllerBase
    {
        private readonly RFIDRC522Driver _rfidDriver;
        private readonly LineSensor _lineSensor;
        private readonly StatusProvider _statusProvider;
        private readonly CollectedObjectsManager _objectManager;
        private readonly LineFollower _lineFollower;

        public SensorsController(RFIDRC522Driver rfiddriver, LineSensor lineSensor, StatusProvider statusProvider, CollectedObjectsManager objectManager, LineFollower lineFollower)
        {
            _rfidDriver = rfiddriver;
            _lineSensor = lineSensor;
            _statusProvider = statusProvider;
            _objectManager = objectManager;
            _lineFollower = lineFollower;
        }

        [HttpPost("rfidupdate")]
        public async Task<IActionResult> PostRFIDUpdate(RFIDCardData body)
        {
            var status = await _statusProvider.GetStatusAsync();
            if (status.Status != (int)StatusContainer.EStatus.Driving)
                return Ok();

            _rfidDriver.UpdateCardData(body.Data);
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
                CalculatedMidpoint = LineSensorsMidpoint.CalculateMidpoint(_lineSensor.SensorValuesLeftRight),
                Settings = new LineFollowerSettings
                { 
                    Speed = _lineFollower.Speed,
                    BackupSpeed = _lineFollower.BackupSpeed,
                    LineDetectionThreshold = _lineFollower.LineDetectionThreshold,
                    LineCenteredRange = _lineFollower.LineCenteredRange
                }
            };
            return Ok(response);
        }
    }
}
