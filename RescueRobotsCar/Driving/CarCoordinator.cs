using RescueRobotsCar.Driving.Sensors;

namespace RescueRobotsCar.Driving
{
    public class CarCoordinator
    {
        public enum CarState
        {
            Idle,
            Stopped,
            FollowingLine,
            TurningOnIntersection,
            Finished
        }

        private readonly CarState _state;
        private readonly Compass _compass;

        public CarCoordinator(Compass compass)
        {
            _compass = compass;
        }

        public void Start() 
        {
        
        }

        public void Pause() 
        { 
        
        }

        public void Continue() 
        {

        }

        public void ImportNewRoute()
        {

        }

        public void Reset()
        {

        }
    }
}
